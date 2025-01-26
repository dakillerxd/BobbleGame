using System.Collections;
using UnityEngine;
using TMPro;


public class TextEffects : MonoBehaviour
{

    private TextMeshProUGUI fontText;

    private float fontSize;
    private float bigFontSize; 
    private float pumpEffectTime = 1f;
    private float enlargeEffectTime = 0.05f;



    void Start()
    {
        fontText = GetComponent<TextMeshProUGUI>();

        fontSize = fontText.fontSize;
        bigFontSize = fontText.fontSize * 1.3f;

    }

    private IEnumerator EnlargeEffectAndFade() {
        
        while (true)
        {
            fontText.fontSize += 1.1f;
            fontText.alpha -= 0.1f;
            yield return new WaitForSeconds(enlargeEffectTime);
        } 
        
    }


    private IEnumerator PumpEffect() {
        
        while (true)
        {
            if (fontText.fontSize == fontSize) {
                
                fontText.fontSize = bigFontSize;
                
            }
            else if (fontText.fontSize == bigFontSize) {

                fontText.fontSize = fontSize;
            }

            yield return new WaitForSeconds(pumpEffectTime);
        } 
        
    }
}
