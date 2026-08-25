[CmdletBinding()]
param(
    [switch]$Open
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$watcherDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$pathsFile = Join-Path $watcherDirectory 'paths.json'
$templateFile = Join-Path $watcherDirectory 'template.html'
$outputFile = Join-Path $watcherDirectory 'GAISWatcher.html'

function Resolve-InputPath {
    param([Parameter(Mandatory)][string]$ConfiguredPath)
    $candidate = if ([System.IO.Path]::IsPathRooted($ConfiguredPath)) {
        $ConfiguredPath
    } else {
        Join-Path $watcherDirectory $ConfiguredPath
    }
    [System.IO.Path]::GetFullPath($candidate)
}

function Get-RelativePathCompat {
    param(
        [Parameter(Mandatory)][string]$BasePath,
        [Parameter(Mandatory)][string]$TargetPath
    )
    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = [Uri]::new($baseFullPath)
    $targetUri = [Uri]::new($targetFullPath)
    [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

function Get-InputFiles {
    param(
        [Parameter(Mandatory)][object[]]$ConfiguredPaths,
        [Parameter(Mandatory)][string]$Extension,
        [switch]$AllowMissing
    )
    $files = [System.Collections.Generic.Dictionary[string, System.IO.FileInfo]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($configuredPath in $ConfiguredPaths) {
        $fullPath = Resolve-InputPath ([string]$configuredPath)
        if (-not (Test-Path -LiteralPath $fullPath)) {
            if ($AllowMissing) { continue }
            throw "Configured path does not exist: $fullPath"
        }
        $item = Get-Item -LiteralPath $fullPath
        $found = if ($item.PSIsContainer) {
            @(Get-ChildItem -LiteralPath $fullPath -Recurse -File -Filter "*$Extension")
        } else {
            if ($item.Extension -ne $Extension) {
                throw "Expected a $Extension file: $fullPath"
            }
            @($item)
        }
        foreach ($file in $found) { $files[$file.FullName] = $file }
    }
    @($files.Values | Sort-Object FullName)
}

function Read-XmlSchema {
    param([Parameter(Mandatory)][System.IO.FileInfo[]]$Files)
    $definitions = [System.Collections.Generic.List[object]]::new()
    $enums = [System.Collections.Generic.List[object]]::new()
    $definitionNames = @{}
    $enumNames = @{}

    foreach ($file in $Files) {
        [xml]$document = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        $moduleNode = $document.SelectSingleNode('/module')
        if ($null -eq $moduleNode) { continue }
        $moduleName = $moduleNode.GetAttribute('name')

        foreach ($bean in @($document.SelectNodes('/module/bean'))) {
            $name = $bean.GetAttribute('name')
            $fullName = if ($moduleName) { "$moduleName.$name" } else { $name }
            if ($definitionNames.ContainsKey($fullName)) { throw "Duplicate Bean: $fullName" }
            $definitionNames[$fullName] = $file.FullName
            $fields = [System.Collections.Generic.List[object]]::new()
            foreach ($field in @($bean.SelectNodes('./var'))) {
                $fields.Add([ordered]@{
                    name = $field.GetAttribute('name')
                    type = $field.GetAttribute('type')
                    comment = $field.GetAttribute('comment')
                })
            }
            $definitions.Add([ordered]@{
                name = $name
                fullName = $fullName
                parent = $bean.GetAttribute('parent')
                comment = $bean.GetAttribute('comment')
                fields = @($fields)
            })
        }

        foreach ($enum in @($document.SelectNodes('/module/enum'))) {
            $name = $enum.GetAttribute('name')
            $fullName = if ($moduleName) { "$moduleName.$name" } else { $name }
            if ($enumNames.ContainsKey($fullName)) { throw "Duplicate Enum: $fullName" }
            $enumNames[$fullName] = $file.FullName
            $values = [System.Collections.Generic.List[object]]::new()
            foreach ($value in @($enum.SelectNodes('./var'))) {
                $values.Add([ordered]@{
                    name = $value.GetAttribute('name')
                    value = $value.GetAttribute('value')
                    alias = $value.GetAttribute('alias')
                })
            }
            $enums.Add([ordered]@{
                name = $name
                fullName = $fullName
                comment = $enum.GetAttribute('comment')
                flags = ($enum.GetAttribute('flags') -eq 'true')
                values = @($values)
            })
        }
    }
    [ordered]@{ definitions = @($definitions); enums = @($enums) }
}

function Read-ConfigRows {
    param(
        [Parameter(Mandatory)][string]$Kind,
        [Parameter(Mandatory)][object[]]$ConfiguredPaths,
        [switch]$AllowMissing
    )
    $rows = [System.Collections.Generic.List[object]]::new()
    $ids = @{}
    foreach ($configuredPath in $ConfiguredPaths) {
        $root = Resolve-InputPath ([string]$configuredPath)
        if (-not (Test-Path -LiteralPath $root)) {
            if ($AllowMissing) { continue }
            throw "Configured path does not exist: $root"
        }
        $rootItem = Get-Item -LiteralPath $root
        $files = if ($rootItem.PSIsContainer) {
            @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.json' | Sort-Object FullName)
        } else { @($rootItem) }

        foreach ($file in $files) {
            $data = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($null -eq $data.PSObject.Properties['id']) { throw "$Kind JSON is missing id: $($file.FullName)" }
            $id = [string]$data.id
            if ($ids.ContainsKey($id)) { throw "Duplicate $Kind id $id in $($file.FullName) and $($ids[$id])" }
            $ids[$id] = $file.FullName
            $unitId = Split-Path -Leaf $file.DirectoryName
            if ($rootItem.PSIsContainer) {
                $relativeDirectory = Get-RelativePathCompat -BasePath $root -TargetPath $file.DirectoryName
                if ($relativeDirectory -and $relativeDirectory -ne '.') {
                    $unitId = ($relativeDirectory -split '[\\/]')[0]
                }
            }
            $rows.Add([ordered]@{
                id = $data.id
                unitId = $unitId
                source = (Get-RelativePathCompat -BasePath $watcherDirectory -TargetPath $file.FullName).Replace('\', '/')
                data = $data
            })
        }
    }
    @($rows)
}

if (-not (Test-Path -LiteralPath $pathsFile)) { throw "Missing paths.json: $pathsFile" }
if (-not (Test-Path -LiteralPath $templateFile)) { throw "Missing template.html: $templateFile" }

$paths = Get-Content -LiteralPath $pathsFile -Raw -Encoding UTF8 | ConvertFrom-Json
$xmlFiles = Get-InputFiles -ConfiguredPaths @($paths.defines) -Extension '.xml'
if ($xmlFiles.Count -eq 0) { throw 'No XML definitions were found.' }
$schema = Read-XmlSchema -Files $xmlFiles

$skills = @(Read-ConfigRows -Kind 'skill' -ConfiguredPaths @($paths.data.skill))
$effects = @(Read-ConfigRows -Kind 'effect' -ConfiguredPaths @($paths.data.effect))

$payload = [ordered]@{
    generatedAt = [DateTime]::Now.ToString('yyyy-MM-dd HH:mm:ss')
    definitions = $schema.definitions
    enums = $schema.enums
    skills = $skills
    effects = $effects
}

$json = $payload | ConvertTo-Json -Depth 100 -Compress
$json = $json -replace '</script', '<\/script'
$template = Get-Content -LiteralPath $templateFile -Raw -Encoding UTF8
if (-not $template.Contains('__GAIS_WATCHER_DATA__')) { throw 'Template data placeholder is missing.' }
$html = $template.Replace('__GAIS_WATCHER_DATA__', $json)
[System.IO.File]::WriteAllText($outputFile, $html, [System.Text.UTF8Encoding]::new($false))

Write-Host "GAISWatcher generated: $outputFile"
Write-Host "Skills: $($skills.Count), Effects: $($effects.Count)"
Write-Host "Beans: $($schema.definitions.Count), Enums: $($schema.enums.Count)"

if ($Open) { Start-Process $outputFile }
