using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public class DamageNumBar : MonoBehaviour
    {
        [SerializeField] private Vector2 _floatOffset = new(0f, 80f);
        [SerializeField] private Vector2 _randomRange = new(30f, 15f);
        [SerializeField] private Vector2 _durationRange = new(0.7f, 0.9f);
        [SerializeField] private Font _font;
        [SerializeField, Min(1)] private int _fontSize = 30;
        [SerializeField] private Color _normalDamageColor = Color.white;
        [SerializeField] private Color _criticalDamageColor = Color.red;
        [SerializeField] private Color _healColor = Color.green;

        private readonly List<FloatingText> _activeTexts = new();
        private readonly Stack<Text> _textPool = new();

        private Font _runtimeFont;

        public void ShowDamage(float damage, bool isCritical = false)
        {
            ShowNumber(damage, "-", isCritical ? _criticalDamageColor : _normalDamageColor);
        }

        public void ShowHeal(float heal)
        {
            ShowNumber(heal, "+", _healColor);
        }

        private void ShowNumber(float value, string prefix, Color color)
        {
            value = Mathf.Abs(value);
            if (value <= 0f)
            {
                return;
            }

            var randomOffset = new Vector2(
                Random.Range(-_randomRange.x, _randomRange.x),
                Random.Range(-_randomRange.y, _randomRange.y));
            var startPosition = randomOffset;
            var text = GetText();
            var duration = Mathf.Max(
                0.01f,
                Random.Range(
                    Mathf.Min(_durationRange.x, _durationRange.y),
                    Mathf.Max(_durationRange.x, _durationRange.y)));

            text.text = $"{prefix}{value:0.#}";
            text.color = color;
            text.rectTransform.anchoredPosition = startPosition;

            _activeTexts.Add(new FloatingText
            {
                Text = text,
                StartPosition = startPosition,
                EndPosition = startPosition + _floatOffset,
                Color = color,
                Duration = duration
            });
        }

        private void Update()
        {
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                var floatingText = _activeTexts[i];
                floatingText.Elapsed += Time.deltaTime;

                var progress = Mathf.Clamp01(floatingText.Elapsed / floatingText.Duration);
                floatingText.Text.rectTransform.anchoredPosition = Vector2.Lerp(
                    floatingText.StartPosition,
                    floatingText.EndPosition,
                    progress);

                var color = floatingText.Color;
                color.a *= 1f - progress;
                floatingText.Text.color = color;

                if (progress < 1f)
                {
                    continue;
                }

                ReleaseText(floatingText.Text);
                _activeTexts.RemoveAt(i);
            }
        }

        private Text GetText()
        {
            if (_textPool.Count > 0)
            {
                var pooledText = _textPool.Pop();
                pooledText.gameObject.SetActive(true);
                return pooledText;
            }

            var textObject = new GameObject(
                "DamageNum",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(Outline));
            textObject.transform.SetParent(transform, false);
            textObject.transform.SetAsLastSibling();

            var text = textObject.GetComponent<Text>();
            text.font = GetFont();
            text.fontSize = _fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            var rectTransform = text.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(160f, 50f);

            var outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            return text;
        }

        private Font GetFont()
        {
            if (_font != null)
            {
                return _font;
            }

            _runtimeFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _runtimeFont;
        }

        private void ReleaseText(Text text)
        {
            text.gameObject.SetActive(false);
            _textPool.Push(text);
        }

        private sealed class FloatingText
        {
            public Text Text;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public Color Color;
            public float Duration;
            public float Elapsed;
        }
    }
}
