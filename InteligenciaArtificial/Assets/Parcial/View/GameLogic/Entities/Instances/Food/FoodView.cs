
using Parcial.Architecture.GameLogic.Entities.Food;
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.Mapping;
using Parcial.View.Scene;
using System;
using TMPro;
using UnityEngine;

[ViewOf(typeof(Food))]
internal class FoodView : EntityView
{
    private const string AMOUNT_LABEL_NAME = "Amount label";
    private const float AMOUNT_LABEL_FONT_SIZE_PER_CELL = 5.0f;

    public override Type ArchitectureEntityType => typeof(Food);

    private TextMeshPro amountLabel;
    private int currentAmount;

    public int CurrentAmount => currentAmount;

    public override void Init()
    {
        base.Init();

        Vector2 cellSize = GameScene.GetCellSize;

        GameObject labelObject = new GameObject(AMOUNT_LABEL_NAME);
        labelObject.transform.SetParent(transform, false);

        Vector3 scale = transform.lossyScale;
        labelObject.transform.localScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f);

        amountLabel = labelObject.AddComponent<TextMeshPro>();
        amountLabel.rectTransform.sizeDelta = cellSize;
        amountLabel.alignment = TextAlignmentOptions.Center;
        amountLabel.textWrappingMode = TextWrappingModes.NoWrap;
        amountLabel.fontSize = cellSize.y * AMOUNT_LABEL_FONT_SIZE_PER_CELL;
        amountLabel.sortingOrder = GameScene.ENTITIES_DRAWING_ORDER + 1;
    }

    public override void LateInit()
    {
        base.LateInit();

        SetAmount(EntityRegistry.GetAs<Food>(archiectureEntitiyID).CurrentAmount);
    }

    public void SetAmount(int amount)
    {
        currentAmount = amount;
        amountLabel.text = amount.ToString();
    }
}
