using System.Collections;
using System.Collections.Generic;
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

    private void ApplyItemEffect(GameObject player)
    {
        BombController bombController = player.GetComponent<BombController>();
        BombermanController bombermanController = player.GetComponent<BombermanController>();

        switch (type)
        {
            case ItemType.BlastRadius:
                bombController.IncreaseBlastRadius();
                break;
            case ItemType.FullBlastRadius:
                bombController.MaximizeBlastRadius();
                break;
            case ItemType.ExtraBomb:
                bombController.AddBomb();
                break;
            case ItemType.PBomb:
                break;
            case ItemType.SpeedIncrease:
                bombermanController.IncreaseSpeed();
                break;
            case ItemType.ExtraLife:
                GameManager.instance.GainExtraLife(1);
                break;
            case ItemType.Kick:
                bombermanController.EnableKick();
                break;
            case ItemType.BoxingGlove:
                bombermanController.EnableBoxingGlove();
                break;
            case ItemType.PowerGlove:
                bombermanController.EnablePowerGlove();
                break;
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyItemEffect(collision.gameObject);
        }
    }
}