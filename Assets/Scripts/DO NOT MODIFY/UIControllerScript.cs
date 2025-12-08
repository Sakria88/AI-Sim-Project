using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class <c>UIControllerScript</c> handles UI.
/// </summary>
public class UIControllerScript : MonoBehaviour
{
    public TextMeshProUGUI tankOneName;
    public TextMeshProUGUI tankOneHealth;
    public TextMeshProUGUI tankOneFuel;
    public TextMeshProUGUI tankOneAmmo;

    public TextMeshProUGUI tankTwoName;
    public TextMeshProUGUI tankTwoHealth;
    public TextMeshProUGUI tankTwoFuel;
    public TextMeshProUGUI tankTwoAmmo;

    public GameObject tankOnePanel;
    public GameObject TankTwoPanel;

    public TextMeshProUGUI winsText;

    public bool showPath = true;
    public bool showObstacles = true;
    public bool showSensor = true;

    public void DisableStatsUi()
    {
        tankOnePanel.SetActive(false);
        TankTwoPanel.SetActive(false);
    }

    public void ShowPath(Toggle toggle)
    {
        showPath = toggle.isOn;
    }

    public void ShowObs(Toggle toggle)
    {
        showObstacles = toggle.isOn;
    }

    public void ShowSensor(Toggle toggle)
    {
        showSensor = toggle.isOn;
    }
}
