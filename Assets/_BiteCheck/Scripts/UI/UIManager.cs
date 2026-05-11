using System;
using System.Reflection;
using BiteCheck.Core;
using BiteCheck.Data;
using BiteCheck.Systems;
using UnityEngine;

namespace BiteCheck.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private Color primaryTextColor = Color.white;
        [SerializeField] private Color feedbackTextColor = new Color(1f, 0.88f, 0.35f);
        [SerializeField] private float feedbackPulseDuration = 0.35f;
        [SerializeField] private int feedbackPulseFontBoost = 10;

        private Component titleLabel;
        private Component statsLabel;
        private Component timerLabel;
        private Component survivorNameLabel;
        private Component ageLabel;
        private Component dialogueLabel;
        private Component symptomsLabel;
        private Component feedbackLabel;
        private Component leftLabel;
        private Component rightLabel;
        private SurvivorCase currentCase;
        private bool missingTextWarningShown;
        private string titleText = "Bite Check";
        private string statsText = string.Empty;
        private string timerText = "Time: --";
        private string survivorNameText = "No survivor";
        private string ageText = "Age: --";
        private string dialogueText = string.Empty;
        private string symptomsText = "Symptoms: --";
        private string feedbackText = "Waiting for survivor...";
        private string leftText = "QUARANTINE";
        private string rightText = "ADMIT";
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle feedbackStyle;
        private GUIStyle actionStyle;
        private float timerPercent;
        private float feedbackPulseTimer;

        private void Awake()
        {
            if (statsManager == null)
            {
                statsManager = FindFirstObjectByType<StatsManager>();
            }

            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            BuildUi();
            RefreshStats();
            ClearCase();
            SetText(feedbackLabel, "Waiting for survivor...");
        }

        private void OnEnable()
        {
            if (statsManager != null)
            {
                statsManager.OnStatsChanged += RefreshStats;
                statsManager.OnGameOver += HandleGameOver;
                statsManager.OnDayComplete += HandleDayComplete;
            }

            if (roundManager != null)
            {
                roundManager.OnSurvivorSpawned += HandleSurvivorSpawned;
                roundManager.OnSurvivorReadyForDecision += HandleSurvivorReadyForDecision;
                roundManager.OnDecisionResolved += HandleDecisionResolved;
                roundManager.OnDecisionTimerChanged += HandleDecisionTimerChanged;
            }
        }

        private void OnDisable()
        {
            if (statsManager != null)
            {
                statsManager.OnStatsChanged -= RefreshStats;
                statsManager.OnGameOver -= HandleGameOver;
                statsManager.OnDayComplete -= HandleDayComplete;
            }

            if (roundManager != null)
            {
                roundManager.OnSurvivorSpawned -= HandleSurvivorSpawned;
                roundManager.OnSurvivorReadyForDecision -= HandleSurvivorReadyForDecision;
                roundManager.OnDecisionResolved -= HandleDecisionResolved;
                roundManager.OnDecisionTimerChanged -= HandleDecisionTimerChanged;
            }
        }

        private void OnGUI()
        {
            EnsureGuiStyles();
            UpdateFeedbackPulseStyle();

            float width = Screen.width;
            float margin = 14f;
            float contentWidth = width - margin * 2f;

            GUI.Label(new Rect(margin, 10f, contentWidth, 42f), titleText, titleStyle);
            GUI.Label(new Rect(margin, 58f, contentWidth, 82f), statsText, bodyStyle);
            GUI.Label(new Rect(margin, 138f, contentWidth, 30f), timerText, feedbackStyle);
            DrawTimerBar(new Rect(margin, 170f, contentWidth, 10f));
            GUI.Label(new Rect(margin, 190f, contentWidth, 38f), survivorNameText, titleStyle);
            GUI.Label(new Rect(margin, 232f, contentWidth, 30f), ageText, bodyStyle);
            GUI.Label(new Rect(margin, 268f, contentWidth, 78f), dialogueText, bodyStyle);
            GUI.Label(new Rect(margin, 350f, contentWidth, 128f), symptomsText, bodyStyle);
            GUI.Label(new Rect(margin, Screen.height - 156f, contentWidth, 76f), feedbackText, feedbackStyle);
            GUI.Label(new Rect(margin, Screen.height - 58f, 170f, 40f), leftText, actionStyle);
            GUI.Label(new Rect(width - margin - 170f, Screen.height - 58f, 170f, 40f), rightText, actionStyle);
        }

        private void BuildUi()
        {
            titleLabel = CreateText("Title", "Bite Check", 44, new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(900f, 70f), primaryTextColor);
            statsLabel = CreateText("Stats", string.Empty, 28, new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(980f, 120f), primaryTextColor);
            timerLabel = CreateText("Timer", timerText, 30, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(920f, 44f), feedbackTextColor);
            survivorNameLabel = CreateText("SurvivorName", string.Empty, 36, new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(920f, 60f), primaryTextColor);
            ageLabel = CreateText("Age", string.Empty, 26, new Vector2(0.5f, 1f), new Vector2(0f, -304f), new Vector2(920f, 44f), primaryTextColor);
            dialogueLabel = CreateText("Dialogue", string.Empty, 28, new Vector2(0.5f, 1f), new Vector2(0f, -382f), new Vector2(920f, 120f), primaryTextColor);
            symptomsLabel = CreateText("Symptoms", string.Empty, 25, new Vector2(0.5f, 1f), new Vector2(0f, -520f), new Vector2(920f, 150f), primaryTextColor);
            feedbackLabel = CreateText("Feedback", string.Empty, 28, new Vector2(0.5f, 0f), new Vector2(0f, 210f), new Vector2(920f, 120f), feedbackTextColor);
            leftLabel = CreateText("LeftLabel", "QUARANTINE", 30, new Vector2(0f, 0f), new Vector2(150f, 72f), new Vector2(280f, 70f), primaryTextColor);
            rightLabel = CreateText("RightLabel", "ADMIT", 30, new Vector2(1f, 0f), new Vector2(-150f, 72f), new Vector2(280f, 70f), primaryTextColor);
        }

        private Component CreateText(string objectName, string text, int fontSize, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = anchor;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Type textType = FindType("TMPro.TextMeshProUGUI", "UnityEngine.UI.Text");
            if (textType == null)
            {
                ShowMissingTextWarning();
                return null;
            }

            Component textComponent = textObject.AddComponent(textType);
            SetText(textComponent, text);
            SetFontSize(textComponent, fontSize);
            SetColor(textComponent, color);
            SetDefaultFontIfNeeded(textComponent);
            return textComponent;
        }

        private void HandleSurvivorSpawned(SurvivorCase survivorCase)
        {
            currentCase = survivorCase;
            RefreshCase();
            feedbackText = "Inspect symptoms, then swipe.";
            SetText(feedbackLabel, feedbackText);
        }

        private void HandleSurvivorReadyForDecision(SurvivorCase survivorCase)
        {
            currentCase = survivorCase;
            RefreshCase();
            feedbackText = "Swipe left to quarantine. Swipe right to admit.";
            SetText(feedbackLabel, feedbackText);
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            feedbackText = result.FeedbackMessage;
            SetText(feedbackLabel, feedbackText);
            PulseFeedbackText();
        }

        private void HandleDecisionTimerChanged(float remaining, float duration)
        {
            timerPercent = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
            timerText = remaining > 0f ? $"Time: {remaining:0.0}s" : "Time: --";
            SetText(timerLabel, timerText);
        }

        private void HandleGameOver()
        {
            feedbackText = "GAME OVER";
            SetText(feedbackLabel, feedbackText);
            PulseFeedbackText();
        }

        private void HandleDayComplete()
        {
            feedbackText = $"DAY {statsManager.Day} COMPLETE";
            SetText(feedbackLabel, feedbackText);
            PulseFeedbackText();
        }

        private void RefreshStats()
        {
            if (statsManager == null)
            {
                statsText = "Day 1\nSecurity 100   Morale 100   Resources 0";
                SetText(statsLabel, statsText);
                return;
            }

            statsText = $"Day {statsManager.Day}\nSecurity {statsManager.Security}   Morale {statsManager.Morale}   Resources {statsManager.Resources}\nSurvivor {Mathf.Min(statsManager.CurrentSurvivorIndex + 1, statsManager.SurvivorsPerDay)}/{statsManager.SurvivorsPerDay}";
            SetText(statsLabel, statsText);
        }

        private void RefreshCase()
        {
            if (currentCase == null)
            {
                ClearCase();
                return;
            }

            survivorNameText = currentCase.DisplayName;
            ageText = $"Age: {currentCase.Age}";
            dialogueText = currentCase.Dialogue;
            symptomsText = $"Symptoms:\n- {string.Join("\n- ", currentCase.Symptoms)}";

            SetText(survivorNameLabel, survivorNameText);
            SetText(ageLabel, ageText);
            SetText(dialogueLabel, dialogueText);
            SetText(symptomsLabel, symptomsText);
        }

        private void ClearCase()
        {
            survivorNameText = "No survivor";
            ageText = "Age: --";
            dialogueText = string.Empty;
            symptomsText = "Symptoms: --";

            SetText(survivorNameLabel, survivorNameText);
            SetText(ageLabel, ageText);
            SetText(dialogueLabel, dialogueText);
            SetText(symptomsLabel, symptomsText);
        }

        private void SetText(Component textComponent, string value)
        {
            if (textComponent != null)
            {
                CacheTextValue(textComponent, value);
            }

            SetProperty(textComponent, "text", value);
        }

        private void CacheTextValue(Component textComponent, string value)
        {
            if (textComponent == titleLabel)
            {
                titleText = value;
            }
            else if (textComponent == statsLabel)
            {
                statsText = value;
            }
            else if (textComponent == timerLabel)
            {
                timerText = value;
            }
            else if (textComponent == survivorNameLabel)
            {
                survivorNameText = value;
            }
            else if (textComponent == ageLabel)
            {
                ageText = value;
            }
            else if (textComponent == dialogueLabel)
            {
                dialogueText = value;
            }
            else if (textComponent == symptomsLabel)
            {
                symptomsText = value;
            }
            else if (textComponent == feedbackLabel)
            {
                feedbackText = value;
            }
            else if (textComponent == leftLabel)
            {
                leftText = value;
            }
            else if (textComponent == rightLabel)
            {
                rightText = value;
            }
        }

        private void SetFontSize(Component textComponent, int value)
        {
            if (textComponent == null)
            {
                return;
            }

            PropertyInfo property = textComponent.GetType().GetProperty("fontSize");
            if (property == null || !property.CanWrite)
            {
                return;
            }

            if (property.PropertyType == typeof(float))
            {
                property.SetValue(textComponent, (float)value);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(textComponent, value);
            }
        }

        private void SetColor(Component textComponent, Color color)
        {
            SetProperty(textComponent, "color", color);
        }

        private void SetDefaultFontIfNeeded(Component textComponent)
        {
            if (textComponent == null || textComponent.GetType().FullName != "UnityEngine.UI.Text")
            {
                return;
            }

            PropertyInfo property = textComponent.GetType().GetProperty("font");
            if (property == null || !property.CanWrite)
            {
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (font != null)
            {
                property.SetValue(textComponent, font);
            }
        }

        private void SetProperty(Component textComponent, string propertyName, object value)
        {
            if (textComponent == null)
            {
                return;
            }

            PropertyInfo property = textComponent.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(textComponent, value);
            }
        }

        private Type FindType(params string[] typeNames)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int typeIndex = 0; typeIndex < typeNames.Length; typeIndex++)
            {
                for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
                {
                    Type type = assemblies[assemblyIndex].GetType(typeNames[typeIndex]);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private void EnsureGuiStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = CreateGuiStyle(22, FontStyle.Bold, primaryTextColor);
            bodyStyle = CreateGuiStyle(15, FontStyle.Normal, primaryTextColor);
            feedbackStyle = CreateGuiStyle(16, FontStyle.Bold, feedbackTextColor);
            actionStyle = CreateGuiStyle(16, FontStyle.Bold, primaryTextColor);
            actionStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawTimerBar(Rect rect)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            Rect fillRect = rect;
            fillRect.width *= timerPercent;
            GUI.color = Color.Lerp(Color.red, Color.green, timerPercent);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void PulseFeedbackText()
        {
            feedbackPulseTimer = feedbackPulseDuration;
            SetFontSize(feedbackLabel, 28 + feedbackPulseFontBoost);
        }

        private void UpdateFeedbackPulseStyle()
        {
            int baseFontSize = 16;

            if (feedbackPulseTimer <= 0f || feedbackPulseDuration <= 0f)
            {
                feedbackStyle.fontSize = baseFontSize;
                SetFontSize(feedbackLabel, 28);
                return;
            }

            feedbackPulseTimer = Mathf.Max(0f, feedbackPulseTimer - Time.deltaTime);
            float pulse = feedbackPulseTimer / feedbackPulseDuration;
            int boostedSize = baseFontSize + Mathf.RoundToInt(feedbackPulseFontBoost * pulse);
            feedbackStyle.fontSize = boostedSize;
            SetFontSize(feedbackLabel, 28 + Mathf.RoundToInt(feedbackPulseFontBoost * pulse));
        }

        private GUIStyle CreateGuiStyle(int fontSize, FontStyle fontStyle, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.normal.textColor = color;
            style.wordWrap = true;
            style.alignment = TextAnchor.UpperCenter;
            return style;
        }

        private void ShowMissingTextWarning()
        {
            if (missingTextWarningShown)
            {
                return;
            }

            missingTextWarningShown = true;
            Debug.LogWarning("UIManager could not find TextMeshProUGUI or UnityEngine.UI.Text. Install TextMeshPro or uGUI to show prototype text.", this);
        }
    }
}
