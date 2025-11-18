using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image fillImage;
    public float maxHP = 30f;

    private float currentHP;

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    public void SetHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        UpdateUI();
    }

    void UpdateUI()
    {
        fillImage.fillAmount = currentHP / maxHP;
        Debug.Log("Decrease by: " + fillImage.fillAmount);
    }
}
