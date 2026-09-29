using UnityEngine;
using UnityEngine.UI;
using LastZone.Core;

namespace LastZone.UI.Screens
{
    public class ResultsScreen : MonoBehaviour
    {
        public Text placementText;
        public Text killsText;
        public Text damageText;

        public void OnReturnToMenu() => Bootstrap.GoToMenu(0f);

        public void SetResults(int placement, int kills, float damage)
        {
            if (placementText != null) placementText.text = $"Placement: {placement}";
            if (killsText != null) killsText.text = $"Kills: {kills}";
            if (damageText != null) damageText.text = $"Damage: {damage:F0}";
        }
    }
}
