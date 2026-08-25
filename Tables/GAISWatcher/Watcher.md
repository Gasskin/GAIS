# BattleConfigWatcher 设计说明

## 1. 目标

BattleConfigWatcher 是一个本地、只读、可生成单文件 HTML 的战斗配置查看器。

它解决的问题是：

- 战斗数据存储在 JSON 中，但 JSON 不包含字段解释。
- 字段结构、类型、注释、Bean 继承和表引用存储在 Luban XML 中。
- 配置之间存在 Skill、Condition、SkillEffect、GameBuff 和嵌套 SkillEffect 链路，平铺 JSON 难以阅读。
- 不同项目的配置目录可能不同，但核心战斗配置结构保持一致。

Watcher 将 XML 作为 Schema 来源，将 JSON 作为数据来源，生成可双击打开的节点画布。

当前范围：

- Skill
- SkillEffect
- Condition
- GameBuff
- SkillEffect 和 GameBuff 中递归引用的 SkillEffect

不读取或修改 XLSX，也不依赖 Markdown 配置说明。

## 2. 文件结构

```text
BattleConfigWatcher/
├─ Build.bat
├─ build.ps1
├─ paths.json
├─ template.html
├─ BattleConfigWatcher.html
└─ Design/
   └─ BattleConfigWatcher设计说明.md
```

各文件职责：

| 文件 | 职责 |
|---|---|
| `Build.bat` | 面向使用者的入口，调用 PowerShell 并打开生成页面。 |
| `build.ps1` | 读取路径配置、XML 和 JSON，校验数据并生成 HTML。 |
| `paths.json` | 配置各类 XML 和 JSON 的实际位置。 |
| `template.html` | 页面样式、节点图构建、画布交互和数据展示逻辑。 |
| `BattleConfigWatcher.html` | 最终生成物，包含 Schema、配置数据、CSS 和 JavaScript。 |

`BattleConfigWatcher.html` 是生成文件。功能修改应落在 `build.ps1` 或 `template.html`，否则再次执行 BAT 时会被覆盖。

## 3. 总体流程

```text
paths.json
    │
    ▼
build.ps1
    ├─ 解析路径
    ├─ 递归收集 XML
    ├─ 编译 Bean / Enum Schema
    ├─ 递归收集 JSON
    ├─ 校验重复 ID 和重复定义
    └─ 将数据注入 template.html
             │
             ▼
BattleConfigWatcher.html
    ├─ 单位下拉框
    ├─ 技能下拉框
    ├─ 递归配置图
    └─ 平移、缩放、框选和组选拖动
```

运行时不需要服务器，也不需要 AI。字段描述来自 XML 的 `comment`，实际值来自 JSON。

## 4. 路径配置

### 4.1 配置格式

```json
{
  "skill": {
    "xml": [
      "../Defines/battle.Table/SkillTable.xml"
    ],
    "json": [
      "../Datas/BattleJson/SkillTable"
    ]
  },
  "skillEffect": {
    "xml": [
      "../Defines/battle.Table/SkillEffectTable.xml"
    ],
    "json": [
      "../Datas/BattleJson/SkillEffectTable"
    ]
  },
  "gameBuff": {
    "xml": [
      "../Defines/battle.Table/GameBuffTable.xml"
    ],
    "json": [
      "../Datas/BattleJson/GameBuffTable"
    ]
  },
  "sharedXml": [
    "../Defines/builtin.xml",
    "../Defines/battle.Bean",
    "../Defines/battle.Enum"
  ]
}
```

### 4.2 路径规则

- 相对路径以 `paths.json` 所在目录为基准。
- 绝对路径可以直接使用。
- 文件路径直接读取该文件。
- 目录路径递归读取所有子目录。
- XML 输入只收集 `.xml`。
- JSON 输入只收集 `.json`。
- 每一个数组均可配置多个数据源。
- 同一个文件被多个路径覆盖时只读取一次。
- 不存在的路径、重复 Bean、重复 Enum 或重复配置 ID 会终止生成。

路径解析的核心形式：

```powershell
$candidate = if ([System.IO.Path]::IsPathRooted($ConfiguredPath)) {
    $ConfiguredPath
}
else {
    Join-Path $ConfigDirectory $ConfiguredPath
}

$fullPath = [System.IO.Path]::GetFullPath($candidate)
```

## 5. 单位与技能分组

单位 ID 不从技能 ID 计算，而是来自 Skill JSON 路径的目录结构。

推荐结构：

```text
SkillTable/
├─ 1101/
│  ├─ 1101101.json
│  └─ 1101102.json
└─ 1102/
   └─ 1102101.json
```

当 `paths.json` 指向 `SkillTable` 时：

- `1101`、`1102` 是单位 ID。
- 各自目录下的 JSON 会递归读取。
- 第一个下拉框显示单位 ID。
- 第二个下拉框只显示该单位目录中的 Skill。

如果配置路径直接指向某个单位目录，则该目录名称作为单位 ID；如果直接指向单个 Skill JSON，则其父目录名称作为单位 ID。

## 6. XML Schema 编译

### 6.1 Bean

Luban Bean：

```xml
<bean name="SkillEffectTable">
    <var name="effectType"
         type="battle.ESkillEffectType"
         comment="效果类型" />
    <var name="effectParam"
         type="battle.BaseEffectParam?"
         comment="效果基类，默认为空" />
</bean>
```

会转换成：

```json
{
  "name": "SkillEffectTable",
  "fullName": "battle.SkillEffectTable",
  "parent": "",
  "comment": "",
  "fields": [
    {
      "name": "effectType",
      "type": "battle.ESkillEffectType",
      "comment": "效果类型"
    },
    {
      "name": "effectParam",
      "type": "battle.BaseEffectParam?",
      "comment": "效果基类，默认为空"
    }
  ]
}
```

页面同时使用简单名称和完整名称建立索引：

```js
for (const definition of DATA.definitions) {
  definitions.set(definition.name, definition);
  definitions.set(definition.fullName, definition);
}
```

### 6.2 Enum

Enum XML 会解析并嵌入 `DATA.enums`：

```xml
<enum name="EDurationType">
    <var name="Instant" value="1" alias="即时" />
    <var name="Duration" value="2" alias="持续" />
</enum>
```

对应结构：

```json
{
  "name": "EDurationType",
  "fullName": "battle.EDurationType",
  "values": [
    { "name": "Instant", "value": "1", "alias": "即时" },
    { "name": "Duration", "value": "2", "alias": "持续" }
  ]
}
```

当前页面主要显示 JSON 中已经导出的 alias。Enum 数据已保留，可继续用于枚举详情、合法性校验或编辑器选项。

### 6.3 多态 Bean

JSON 使用 `$type` 指定实际子类：

```json
{
  "effectParam": {
    "$type": "AddBuffEffectParam",
    "buff": 1101501
  }
}
```

解析字段时优先使用 `$type`：

```js
const typeName = value.$type || simpleTypeName(declaredType);
const definition = definitions.get(typeName);
```

父类字段会与子类字段合并，子类同名字段覆盖父类：

```js
function definitionFields(definition, visited = new Set()) {
  if (!definition || visited.has(definition.fullName || definition.name)) return [];
  const nextVisited = new Set(visited);
  nextVisited.add(definition.fullName || definition.name);
  const parent = definition.parent
    ? definitions.get(definition.parent) || definitions.get(simpleTypeName(definition.parent))
    : null;
  const combined = [...definitionFields(parent, nextVisited), ...(definition.fields || [])];
  const byName = new Map();
  for (const field of combined) byName.set(field.name, field);
  return [...byName.values()];
}
```

## 7. 关系发现

### 7.1 基础链路

```text
Skill → SkillEffect
```

Skill 的 `skillEffect` 与 `effectTime` 按数组下标对应。

### 7.2 Condition

当 SkillEffect 的以下任一字段非空时：

```text
conditions
targetConditions
```

链路变为：

```text
父节点 → Condition → SkillEffect
```

两种条件合并显示在一个 Condition 节点中：

- `conditions`：自身条件。
- `targetConditions`：目标条件。

### 7.3 Effect 到 GameBuff

引用不依赖字段名，而是读取 XML 类型中的表引用：

```xml
<var name="buff"
     type="int#ref=TbGameBuffTable?"
     comment="BuffId，关联GameBuffTable" />
```

递归扫描函数根据：

```text
#ref=TbGameBuffTable
```

发现 GameBuff ID，并生成：

```text
SkillEffect → GameBuff
```

这同时适用于 AddBuff、RemoveBuff 或未来其他引用 GameBuffTable 的 Bean。

### 7.4 嵌套 SkillEffect

以下类型的引用都会继续加入图中：

```text
#ref=TbSkillEffectTable
```

来源可能是：

- `GameBuff.periodEffect`
- `childEffectIds`
- `effects`
- `enterEffect`
- `entityInsideEffect`
- 其他 Bean 内定义的 SkillEffect 引用

例如：

```text
SkillEffect
  → GameBuff
      → periodEffect
          → Condition（可选）
          → SkillEffect
```

以及：

```text
SkillEffect
  → childEffectIds
      → Condition（可选）
      → SkillEffect
```

引用扫描的核心逻辑：

```js
function collectTableReferences(value, declaredType, tableName, references) {
  if (value === null || value === undefined) return;

  if (String(declaredType || "").includes(`#ref=${tableName}`)) {
    const values = Array.isArray(value) ? value : [value];
    for (const item of values) {
      if (item !== 0 && item !== "0" && item !== "") references.push(Number(item));
    }
    return;
  }

  if (Array.isArray(value)) {
    for (const item of value) {
      collectTableReferences(item, elementTypeOf(declaredType), tableName, references);
    }
    return;
  }

  if (typeof value !== "object") return;

  const typeName = value.$type || simpleTypeName(declaredType);
  const definition = definitions.get(typeName);
  if (!definition) return;

  const fields = new Map(
    definitionFields(definition).map((field) => [field.name, field]),
  );

  for (const [name, item] of Object.entries(value)) {
    if (name === "$type") continue;
    const field = fields.get(name);
    if (field) collectTableReferences(item, field.type, tableName, references);
  }
}
```

## 8. 递归图构建

Effect 和 Buff 分别使用全局索引：

```js
const effectGraphEntries = new Map();
const buffGraphEntries = new Map();
```

创建 Effect 前先检查是否已经存在：

```js
const existing = effectGraphEntries.get(effectId);
if (existing) {
  connectNodes(parentNodeId, existing.entryNodeId);
  return existing.effectNodeId;
}
```

这样能够：

- 防止同一配置重复创建大量节点。
- 防止 Effect → Buff → Effect 循环导致无限递归。
- 让多个父节点共享同一个下游配置节点。
- 将循环关系表现为连回已有节点的边。

连接也会去重：

```js
function connectNodes(from, to) {
  if (!from || !to) return;
  if (connections.some((edge) => edge.from === from && edge.to === to)) return;
  connections.push({ from, to });
}
```

## 9. 节点模型

每个节点保存：

```js
{
  id,
  depth,
  x,
  y,
  width,
  height,
  element
}
```

节点类型及颜色：

| 类型 | 颜色 | 内容 |
|---|---|---|
| Skill | 蓝色 | 技能字段、类型、冷却和效果数量。 |
| Condition | 橙色 | 自身条件、目标条件及实际条件 Bean。 |
| SkillEffect | 绿色 | 效果类型、EffectParam 和触发时间。 |
| GameBuff | 紫色 | 持续时间、堆叠、周期效果和属性修改。 |

标题栏、节点边框、圆点和输出端口共同使用节点主题色。

## 10. 自动布局

节点按递归深度分列：

```text
depth 0     depth 1       depth 2       depth 3
Skill    → Condition  → SkillEffect → GameBuff
```

无 Condition 时，SkillEffect 直接占用下一列。

布局流程：

1. 按 `depth` 将节点分组。
2. 计算每列节点总高度。
3. 使用最高列作为整体高度。
4. 每列垂直居中。
5. 节点之间保留固定横向和纵向间距。
6. 生成 SVG 贝塞尔连线。
7. 自动执行“适应屏幕”。

核心坐标计算：

```js
node.x = 80 + depth * (columnWidth + columnGap);
node.y = y;
```

## 11. 画布实现

画布不是将文字绘制进 `<canvas>` 像素层，而是：

```text
DOM 节点 + SVG 连线 + CSS 变换
```

优点：

- 文字可以选择和复制。
- 节点内容可以滚动和展开。
- XML 注释可以正常排版。
- SVG 连线容易实时更新。
- 无需第三方前端框架。

### 11.1 缩放

优先使用 CSS `zoom`，保持放大后的文字清晰：

```js
if (supportsCssZoom) {
  surface.style.zoom = String(view.scale);
  surface.style.transform = "none";
} else {
  surface.style.zoom = "1";
  surface.style.transform = `scale(${view.scale})`;
}
```

不支持 `zoom` 的浏览器才回退到 `transform: scale()`。

### 11.2 交互

- 滚轮：缩放。
- 中键拖动：平移画布。
- `Alt + 左键`：平移画布。
- 左键拖动画布空白处：框选节点。
- `Ctrl`、`Shift` 或 `Command` 点击节点标题：追加或取消选择。
- 拖动任意已选节点标题：整组选中节点一起移动。
- 选中节点提高 `z-index`，显示在未选节点上方。
- 适应屏幕按钮：重新计算全部节点的可视范围。

## 12. 静态 HTML 生成

模板包含占位符：

```js
const DATA = __BATTLE_WATCHER_DATA__;
```

PowerShell 将 Schema 和配置压缩成 JSON 后替换占位符：

```powershell
$json = $payload | ConvertTo-Json -Depth 100 -Compress
$json = $json -replace '</script', '<\/script'
$html = $template.Replace('__BATTLE_WATCHER_DATA__', $json)
```

最终 HTML 不再依赖 XML、JSON、PowerShell 或本地服务，可以单独打开和分发。它是生成时的数据快照；源配置变化后需要重新执行 `Build.bat`。

## 13. 数据校验

生成阶段执行以下校验：

- 配置文件必须存在。
- 配置的每个路径必须存在。
- 文件扩展名必须符合输入类型。
- 配置目录中必须至少找到一个目标文件。
- Bean 完整名称不能重复。
- Enum 完整名称不能重复。
- Skill ID 不能重复。
- SkillEffect ID 不能重复。
- GameBuff ID 不能重复。
- JSON 必须包含 `id`。

缺失的下游引用不会阻止 HTML 生成，而是在画布中显示红色“引用缺失”节点，便于定位数据问题。

## 14. 迁移到其他项目

如果其他项目仍然使用相同的 Luban XML 结构和相同的业务字段，通常只需：

1. 复制整个 `BattleConfigWatcher` 文件夹。
2. 修改 `paths.json`。
3. 双击 `Build.bat`。

可以变化的内容：

- XML 文件名。
- XML 和 JSON 所在目录。
- 是否拆分到多个数据根。
- Bean 和 Enum 文件的子目录层级。

当前仍然固定的业务约定：

- 表 Bean 名为 `SkillTable`、`SkillEffectTable`、`GameBuffTable`。
- Skill 字段使用 `skillEffect`、`effectTime`、`toSelfSkillEffect`。
- Condition 字段使用 `conditions`、`targetConditions`。
- JSON 使用 `id` 作为主键。
- 多态 Bean 使用 `$type`。
- 表引用使用 Luban 的 `#ref=...`。

如果另一个项目连这些字段和 Bean 名也不同，需要修改模板中的逻辑名称，或者再引入字段映射配置。本设计刻意没有把路径配置扩展成完整的 Schema 映射语言，以保持工具简单。

## 15. 已知限制与扩展方向

### 当前限制

- RougeEffect 尚未加入画布。
- Enum 已解析，但尚未在字段旁展示完整候选项。
- 大型递归图可能产生很高的列，初次“适应屏幕”后缩放比例较小。
- 所有 `#ref=TbSkillEffectTable` 都视为图关系；如果某字段只是过滤引用而不是触发关系，需要增加语义过滤规则。
- 静态 HTML 是快照，不会自动监听源文件变化。

### 可选扩展

- 增加节点搜索和 ID 定位。
- 增加链路类型标签，例如“周期触发”“进入范围”“命中触发”。
- 在线条上显示引用字段名。
- 增加 Enum 详情和非法值检查。
- 增加 RougeEffect 节点。
- 输出引用缺失、重复 ID 和循环关系报告。
- 增加只展开选中分支或折叠子树功能。
- 将生成步骤接入 Luban 导出脚本或 CI。

## 16. 修改指南

### 修改输入路径

编辑：

```text
paths.json
```

### 修改数据读取和校验

编辑：

```text
build.ps1
```

### 修改节点、链路、布局和交互

编辑：

```text
template.html
```

### 重新生成

执行：

```text
Build.bat
```

不要直接维护：

```text
BattleConfigWatcher.html
```

它应始终被视为可重新生成的产物。
