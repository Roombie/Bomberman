using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        BlastRadius,
        FullBlastRadius,
        ExtraBomb,
        PBomb,
        SpeedIncrease,
        ExtraLife,
        Kick,
        BoxingGlove,
        PowerGlove
    }

    public ItemType type;
    [SerializeField] private LayerMask playerLayer;

    private void ApplyItemEffect(GameObject player)
    {
        BombController bombController = player.GetComponent<BombController>();
        BombermanController bombermanController = player.GetComponent<BombermanController>();

        if (bombController == null && bombermanController == null)
        {
            Debug.LogError($"Player {player.name} is missing required components!");
            return;
        }

        switch (type)
        {
            case ItemType.BlastRadius:
                bombController?.IncreaseBlastRadius();
                break;
            case ItemType.FullBlastRadius:
                bombController?.MaximizeBlastRadius();
                break;
            case ItemType.ExtraBomb:
                bombController?.AddBomb();
                break;
            case ItemType.PBomb:
                // Handle PBomb logic here
                break;
            case ItemType.SpeedIncrease:
                bombermanController?.IncreaseSpeed();
                break;
            case ItemType.ExtraLife:
                GameManager.instance.GainExtraLife(player, 1);
                break;
            case ItemType.Kick:
                bombermanController?.EnableAbility(AbilityType.Kick);
                break;
            case ItemType.BoxingGlove:
                bombermanController?.EnableAbility(AbilityType.BoxingGlove);
                break;
            case ItemType.PowerGlove:
                bombermanController?.EnableAbility(AbilityType.PowerGlove);
                break;
        }

        Debug.Log($"Applied {type} to {player.name}");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ApplyItemEffect(collision.gameObject);
        }
    }
}