using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RobotAnimator : MonoBehaviour
{
    public Image robotImage;

    [Header("Sprites normales")]
    public Sprite closedMouth;
    public Sprite openMouth;

    [Header("Sprites tristes")]
    public Sprite sadClosedMouth;
    public Sprite sadOpenMouth;

    [Header("Velocidad")]
    public float animationSpeed = 0.3f;

    private Coroutine currentAnimation;

    // --- Animación normal ---
    public void StartTalking()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(TalkAnimation(closedMouth, openMouth));
    }

    // --- Animación triste ---
    public void StartTalkingSad()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(TalkAnimation(sadClosedMouth, sadOpenMouth));
    }

    // --- Detener cualquier animación ---
    public void StopTalking()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        robotImage.sprite = closedMouth; // por defecto vuelve a cara normal cerrada
    }

    // --- Corrutina generalizada ---
    private IEnumerator TalkAnimation(Sprite closed, Sprite open)
    {
        while (true)
        {
            robotImage.sprite = open;
            yield return new WaitForSeconds(animationSpeed);

            robotImage.sprite = closed;
            yield return new WaitForSeconds(animationSpeed);
        }
    }
}