using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitPlaygroundBehaviour : MonoBehaviour
{
    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) return;
        var root = doc.rootVisualElement;

        var scoreLabel  = root.Q<Label>("score-label");
        var statusLabel = root.Q<Label>("status-label");
        int score = 1250;

        root.Q<Button>("start-button").clicked    += () => statusLabel.text = "Starting game...";
        root.Q<Button>("settings-button").clicked += () => statusLabel.text = "Opening settings...";
        root.Q<Button>("credits-button").clicked  += () => statusLabel.text = "Showing credits...";
        root.Q<Button>("quit-button").clicked     += () => statusLabel.text = "Quitting...";

        root.Q<Button>("add-score-button").clicked += () =>
        {
            score += 100;
            scoreLabel.text = "Score: " + score;
        };
        root.Q<Button>("reset-score-button").clicked += () =>
        {
            score = 0;
            scoreLabel.text = "Score: 0";
        };
    }
}
