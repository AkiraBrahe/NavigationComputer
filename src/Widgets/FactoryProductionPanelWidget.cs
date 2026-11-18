using BattleTech;
using BattleTech.UI.Tooltips;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationComputer.Widgets
{
    public class FactoryProductionPanelWidget : MonoBehaviour
    {
        private RectTransform content;
        private TMP_FontAsset font;
        private float fontSize = 14f;
        private Color fontColor = Color.gray;

        private string systemId;

        public bool Initialize()
        {
            if (content != null) return true;

            var scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (scrollRect == null || scrollRect.content == null)
            {
                Main.Log.LogError("Factory production panel is missing its ScrollRect content.");
                return false;
            }

            content = scrollRect.content;
            var sampleText = content.GetComponentInChildren<TextMeshProUGUI>(true)
                ?? GetComponentInChildren<TextMeshProUGUI>(true);

            if (sampleText != null)
            {
                font = sampleText.font;
                fontSize = sampleText.fontSize;
                fontColor = sampleText.color;
            }

            var layout = content.GetComponent<VerticalLayoutGroup>()
                ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>()
                ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);

            ClearRows();
            return true;
        }

        public void SetSystem(string systemId)
        {
            this.systemId = systemId;
        }

        public void ShowProduction(string systemId, IEnumerable<MechDef> mechs, IEnumerable<VehicleDef> vehicles)
        {
            if (this.systemId != systemId) return;

            ClearRows();
            bool hasProduction = false;

            var mechList = mechs
                .GroupBy(def => def.Description.Id)
                .Select(group => group.First())
                .OrderBy(def => def.Description.Id)
                .ToList();
            if (mechList.Count > 0)
            {
                AddRow("Mechs", true);
                foreach (var mech in mechList)
                {
                    AddRow(mech.Description.UIName, false, BuildDescription(mech));
                }

                hasProduction = true;
            }

            var vehicleList = vehicles
                .GroupBy(def => def.Description.Id)
                .Select(group => group.First())
                .OrderBy(def => def.Description.Id)
                .ToList();
            if (vehicleList.Count > 0)
            {
                AddRow("Vehicles", true);
                foreach (var vehicle in vehicleList)
                {
                    AddRow(vehicle.Chassis.Description.Name, false, BuildDescription(vehicle));
                }

                hasProduction = true;
            }

            if (!hasProduction)
            {
                AddRow("No factory production found.", false);
            }
        }

        public void HideProduction() => ClearRows();

        private void ClearRows()
        {
            if (content == null) return;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        private void AddRow(string text, bool isHeader, BaseDescriptionDef descriptionDef = null)
        {
            var rowObject = new GameObject("row", typeof(RectTransform), typeof(TextMeshProUGUI));
            rowObject.transform.SetParent(content, false);

            var row = rowObject.GetComponent<TextMeshProUGUI>();
            row.font = font;
            row.fontSize = isHeader ? fontSize * 1.1f : fontSize;
            row.color = isHeader ? fontColor : Color.gray;
            row.alignment = TextAlignmentOptions.TopLeft;
            row.enableWordWrapping = true;
            row.raycastTarget = !isHeader;
            row.text = isHeader ? $"<b>{text}</b>" : $"• {text}";

            var rectTransform = row.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);

            var fitter = rowObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (descriptionDef != null)
            {
                var tooltip = rowObject.AddComponent<HBSTooltip>();
                tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(descriptionDef));
            }
        }

        private static BaseDescriptionDef BuildDescription(MechDef mechDef)
        {
            return new BaseDescriptionDef(mechDef.Description.Id, mechDef.Description.UIName, mechDef.Description.Details, null);
        }

        private static BaseDescriptionDef BuildDescription(VehicleDef vehicleDef)
        {
            return new BaseDescriptionDef(vehicleDef.Description.Id, vehicleDef.Chassis.Description.Name, RemoveVehicleInfo(vehicleDef.Description.Details), null);
        }

        private static string RemoveVehicleInfo(string details)
        {
            int infoIndex = details.IndexOf("</b>\n\n");
            return infoIndex > 0 ? details.Substring(infoIndex + 6).TrimStart() : details;
        }
    }
}