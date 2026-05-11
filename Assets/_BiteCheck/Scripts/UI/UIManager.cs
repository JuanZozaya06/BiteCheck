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
        private enum UiFlowState
        {
            MainMenu,
            Playing,
            DaySummary,
            GameOver
        }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private StatsManager statsManager;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private Color primaryTextColor = Color.white;
        [SerializeField] private Color feedbackTextColor = new Color(1f, 0.88f, 0.35f);
        [SerializeField] private float feedbackPulseDuration = 0.35f;
        [SerializeField] private int feedbackPulseFontBoost = 14;

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
        private GUIStyle panelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle overlayTitleStyle;
        private GUIStyle hudPanelStyle;
        private float timerPercent;
        private float feedbackPulseTimer;
        private bool showingDaySummary;
        private DaySummary activeDaySummary;
        private UiFlowState flowState = UiFlowState.MainMenu;

        private void Awake()
        {
            if (statsManager == null)
            {
                statsManager = FindFirstObjectByType<StatsManager>();
            }

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            }

            BuildUi();
            RefreshStats();
            ClearCase();
            SetText(feedbackLabel, "Waiting for survivor...");
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnRunStarted += HandleRunStarted;
                gameManager.OnDayStarted += HandleDayStarted;
                gameManager.OnDaySummaryReady += HandleDaySummaryReady;
            }

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

            if (upgradeSystem != null)
            {
                upgradeSystem.OnUpgradesChanged += HandleUpgradesChanged;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnRunStarted -= HandleRunStarted;
                gameManager.OnDayStarted -= HandleDayStarted;
                gameManager.OnDaySummaryReady -= HandleDaySummaryReady;
            }

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

            if (upgradeSystem != null)
            {
                upgradeSystem.OnUpgradesChanged -= HandleUpgradesChanged;
            }
        }

        private void OnGUI()
        {
            EnsureGuiStyles();
            UpdateFeedbackPulseStyle();

            Rect safeArea = GetSafeAreaRect();
            float width = safeArea.width;
            float margin = 14f;
            float contentWidth = width - margin * 2f;
            float originX = safeArea.x;
            float originY = safeArea.y;

            if (flowState == UiFlowState.MainMenu)
            {
                DrawMainMenu(safeArea);
                return;
            }

            DrawHudPanels(safeArea, margin);
            GUI.Label(new Rect(originX + margin, originY + 10f, contentWidth, 42f), titleText, titleStyle);
            GUI.Label(new Rect(originX + margin, originY + 58f, contentWidth, 82f), statsText, bodyStyle);
            GUI.Label(new Rect(originX + margin, originY + 138f, contentWidth, 30f), timerText, feedbackStyle);
            DrawTimerBar(new Rect(originX + margin, originY + 170f, contentWidth, 10f));
            GUI.Label(new Rect(originX + margin, originY + 190f, contentWidth, 38f), survivorNameText, titleStyle);
            GUI.Label(new Rect(originX + margin, originY + 232f, contentWidth, 30f), ageText, bodyStyle);
            GUI.Label(new Rect(originX + margin, originY + 268f, contentWidth, 78f), dialogueText, bodyStyle);
            GUI.Label(new Rect(originX + margin, originY + 350f, contentWidth, 128f), symptomsText, bodyStyle);
            GUI.Label(new Rect(originX + margin, safeArea.yMax - 158f, contentWidth, 78f), feedbackText, feedbackStyle);
            GUI.Label(new Rect(originX + margin, safeArea.yMax - 58f, 170f, 40f), leftText, actionStyle);
            GUI.Label(new Rect(safeArea.xMax - margin - 170f, safeArea.yMax - 58f, 170f, 40f), rightText, actionStyle);

            if (flowState == UiFlowState.GameOver)
            {
                DrawGameOver(safeArea);
                return;
            }

            if (showingDaySummary && activeDaySummary != null)
            {
                DrawDaySummary(safeArea);
            }
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
            showingDaySummary = false;
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
            feedbackText = GetFeedbackDisplayText(result);
            SetText(feedbackLabel, feedbackText);
            PulseFeedbackText();
        }

        private void HandleDecisionTimerChanged(float remaining, float duration)
        {
            timerPercent = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
            timerText = remaining > 0f ? $"Time: {remaining:0.0}s" : "Time: --";
            if (remaining > 0f && remaining <= 1.5f)
            {
                timerText = $"DECIDE NOW: {remaining:0.0}s";
            }

            SetText(timerLabel, timerText);
        }

        private void HandleGameOver()
        {
            flowState = UiFlowState.GameOver;
            showingDaySummary = false;
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

        private void HandleRunStarted()
        {
            flowState = UiFlowState.Playing;
            showingDaySummary = false;
            activeDaySummary = null;
        }

        private void HandleDayStarted(int day)
        {
            flowState = UiFlowState.Playing;
            showingDaySummary = false;
            activeDaySummary = null;
            feedbackText = $"Day {day}. Check them fast.";
            SetText(feedbackLabel, feedbackText);
            PulseFeedbackText();
        }

        private void HandleDaySummaryReady(DaySummary summary)
        {
            flowState = UiFlowState.DaySummary;
            activeDaySummary = summary;
            showingDaySummary = true;
            ClearCase();
            timerText = "Time: --";
            timerPercent = 0f;
            SetText(timerLabel, timerText);
            feedbackText = $"DAY {summary.DayCompleted} COMPLETE";
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

            int visibleClues = Mathf.Min(currentCase.Symptoms.Count, 2 + (upgradeSystem != null ? upgradeSystem.ExtraVisibleClues : 0));
            string[] visibleSymptoms = new string[visibleClues];
            for (int i = 0; i < visibleClues; i++)
            {
                visibleSymptoms[i] = currentCase.Symptoms[i];
            }

            survivorNameText = currentCase.DisplayName;
            ageText = $"Age: {currentCase.Age}";
            dialogueText = currentCase.Dialogue;
            symptomsText = $"Symptoms:\n- {string.Join("\n- ", visibleSymptoms)}";

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

        private void HandleUpgradesChanged()
        {
            RefreshStats();
            RefreshCase();
        }

        private string GetFeedbackDisplayText(DecisionResult result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            if (result.Correct)
            {
                return $"CORRECT\n{result.FeedbackMessage}";
            }

            return result.ResultType == DecisionResultType.WrongAdmitInfected
                ? $"SECURITY HIT\n{result.FeedbackMessage}"
                : $"MORALE HIT\n{result.FeedbackMessage}";
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
            bodyStyle = CreateGuiStyle(16, FontStyle.Normal, primaryTextColor);
            feedbackStyle = CreateGuiStyle(18, FontStyle.Bold, feedbackTextColor);
            actionStyle = CreateGuiStyle(17, FontStyle.Bold, primaryTextColor);
            actionStyle.alignment = TextAnchor.MiddleCenter;
            subtitleStyle = CreateGuiStyle(16, FontStyle.Normal, primaryTextColor);
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            overlayTitleStyle = CreateGuiStyle(34, FontStyle.Bold, primaryTextColor);
            overlayTitleStyle.alignment = TextAnchor.MiddleCenter;
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.textColor = primaryTextColor;
            hudPanelStyle = new GUIStyle(GUI.skin.box);
            hudPanelStyle.normal.background = Texture2D.whiteTexture;
            hudPanelStyle.normal.textColor = primaryTextColor;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 18;
            buttonStyle.fontStyle = FontStyle.Bold;
        }

        private void DrawHudPanels(Rect safeArea, float margin)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.Box(new Rect(safeArea.x + margin * 0.5f, safeArea.y + 6f, safeArea.width - margin, 176f), string.Empty, hudPanelStyle);
            GUI.Box(new Rect(safeArea.x + margin * 0.5f, safeArea.y + 184f, safeArea.width - margin, 300f), string.Empty, hudPanelStyle);
            GUI.Box(new Rect(safeArea.x + margin * 0.5f, safeArea.yMax - 166f, safeArea.width - margin, 152f), string.Empty, hudPanelStyle);
            GUI.color = previousColor;
        }

        private Rect GetSafeAreaRect()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return new Rect(0f, 0f, Screen.width, Screen.height);
            }

            return new Rect(
                safeArea.x,
                Screen.height - safeArea.yMax,
                safeArea.width,
                safeArea.height);
        }

        private void DrawMainMenu(Rect safeArea)
        {
            float panelWidth = Mathf.Min(safeArea.width - 28f, 390f);
            float panelHeight = 300f;
            Rect panelRect = new Rect(
                safeArea.x + (safeArea.width - panelWidth) * 0.5f,
                safeArea.y + (safeArea.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty, panelStyle);

            float x = panelRect.x + 18f;
            float y = panelRect.y + 28f;
            float width = panelRect.width - 36f;

            GUI.Label(new Rect(x, y, width, 58f), "Bite Check", overlayTitleStyle);
            y += 72f;
            GUI.Label(new Rect(x, y, width, 78f), "Spot the infected. Swipe fast. Don't doom the shelter.", subtitleStyle);
            y += 104f;

            if (GUI.Button(new Rect(x, y, width, 56f), "START", buttonStyle))
            {
                gameManager?.StartRun();
            }
        }

        private void DrawGameOver(Rect safeArea)
        {
            float panelWidth = Mathf.Min(safeArea.width - 28f, 380f);
            float panelHeight = 330f;
            Rect panelRect = new Rect(
                safeArea.x + (safeArea.width - panelWidth) * 0.5f,
                safeArea.y + (safeArea.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty, panelStyle);

            float x = panelRect.x + 18f;
            float y = panelRect.y + 24f;
            float width = panelRect.width - 36f;

            GUI.Label(new Rect(x, y, width, 52f), "Game Over", overlayTitleStyle);
            y += 68f;

            string summary = statsManager == null
                ? "The shelter did not make it."
                : $"Day reached: {statsManager.Day}\nSecurity: {statsManager.Security}\nMorale: {statsManager.Morale}\nResources: {statsManager.Resources}";

            GUI.Label(new Rect(x, y, width, 120f), summary, bodyStyle);
            y += 150f;

            if (GUI.Button(new Rect(x, y, width, 56f), "RESTART", buttonStyle))
            {
                gameManager?.RestartRun();
            }
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

        private void DrawDaySummary(Rect safeArea)
        {
            float panelWidth = Mathf.Min(safeArea.width - 28f, 360f);
            float panelHeight = 450f;
            Rect panelRect = new Rect(
                safeArea.x + (safeArea.width - panelWidth) * 0.5f,
                safeArea.y + (safeArea.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, string.Empty, panelStyle);

            float x = panelRect.x + 18f;
            float y = panelRect.y + 18f;
            float width = panelRect.width - 36f;

            GUI.Label(new Rect(x, y, width, 34f), $"Day {activeDaySummary.DayCompleted} completed", titleStyle);
            y += 48f;
            GUI.Label(
                new Rect(x, y, width, 150f),
                $"Correct decisions: {activeDaySummary.CorrectDecisions}\nWrong decisions: {activeDaySummary.WrongDecisions}\nSecurity: {activeDaySummary.Security}\nMorale: {activeDaySummary.Morale}\nResources earned: {activeDaySummary.ResourcesEarned}\nTotal resources: {activeDaySummary.TotalResources}",
                bodyStyle);
            y += 160f;

            DrawUpgradeButton(
                new Rect(x, y, width, 38f),
                "Better Scanner",
                UpgradeSystem.BetterScannerCost,
                upgradeSystem != null && upgradeSystem.BetterScannerOwned,
                upgradeSystem != null && upgradeSystem.CanBuyBetterScanner(),
                () => upgradeSystem.BuyBetterScanner());
            y += 44f;

            DrawUpgradeButton(
                new Rect(x, y, width, 38f),
                "Reinforced Gate",
                UpgradeSystem.ReinforcedGateCost,
                upgradeSystem != null && upgradeSystem.ReinforcedGateOwned,
                upgradeSystem != null && upgradeSystem.CanBuyReinforcedGate(),
                () => upgradeSystem.BuyReinforcedGate());
            y += 44f;

            DrawUpgradeButton(
                new Rect(x, y, width, 38f),
                "Public Trust",
                UpgradeSystem.PublicTrustCost,
                upgradeSystem != null && upgradeSystem.PublicTrustOwned,
                upgradeSystem != null && upgradeSystem.CanBuyPublicTrust(),
                () => upgradeSystem.BuyPublicTrust());
            y += 52f;

            if (GUI.Button(new Rect(x, y, width, 50f), "CONTINUE", buttonStyle))
            {
                showingDaySummary = false;
                flowState = UiFlowState.Playing;
                gameManager?.ContinueToNextDay();
            }
        }

        private void DrawUpgradeButton(Rect rect, string upgradeName, int cost, bool owned, bool canBuy, Action buyAction)
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = !owned && canBuy;

            string label = owned ? $"{upgradeName}: OWNED" : $"{upgradeName}: {cost} resources";
            if (GUI.Button(rect, label, buttonStyle))
            {
                buyAction?.Invoke();
            }

            GUI.enabled = previousEnabled;
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

    }
}
