using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class UpdateScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshPro[] scoreText;
    [SerializeField] private TextMeshPro[] bubblesText;
    private Sequence _updateScoreSequence;
    private Sequence _updateBubbleSequence;

    private void OnEnable()
    {
        SessionManager.OnBubbleLeftUpdate.AddListener(SetBubbleText);
    }

    private void OnDisable()
    {
        SessionManager.OnBubbleLeftUpdate.RemoveListener(SetBubbleText);
    }

    private void SetScoreText(int score)
    {
        if (scoreText.Length <= 0) return;

        foreach (TextMeshPro text in scoreText)
        {
            
            text.text = $"<sketchy>{score}</>";
            Sequence.Create()
                .Group(Tween.PunchScale(text.transform, strength: text.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f));
        }
    }

    private void SetBubbleText(int amount)
    {
        if (bubblesText.Length <= 0) return;
        
        foreach (TextMeshPro text in bubblesText)
        {
            text.text = $"<sketchy>{amount}</>";
            Sequence.Create()
                .Group(Tween.PunchScale(text.transform, strength: text.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f));
        }
    }
}
