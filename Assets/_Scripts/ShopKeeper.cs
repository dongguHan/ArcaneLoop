using System.Collections;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    [Header("Talk Text")]
    public GameObject text;
    public float range;
    private bool isTalking = false;

    [Header("Player Manager")]
    public PlayerManager playerManager;
    private GameObject closePlayer;

    void Start()
    {
        text.SetActive(false);
    }

    void Update()
    {
        FindClosePlayer();

        if (closePlayer != null)
        {
            if (!isTalking)
            {
                StopCoroutine(EndTalk());
                StartCoroutine(StartTalk());
            }
            else
            {
                StopCoroutine(StartTalk());
                StartCoroutine(EndTalk());
            }
        }
    }

    void FindClosePlayer()
    {
        GameObject player = null;

        if (playerManager.isTransform == false)
        {
            float distanceToBlack = Vector2.Distance(transform.position, playerManager.playerBlack.transform.position);
            float distanceToWhite = Vector2.Distance(transform.position, playerManager.playerWhite.transform.position);

            player = distanceToBlack < distanceToWhite ? playerManager.playerBlack : playerManager.playerWhite;
        }
        else
        {
            player = playerManager.playerGray;
        }

        if (closePlayer != player || closePlayer == null)
        {
            closePlayer = player;
            // Debug.Log(player.tag.ToString());
        }
        else
        {
            return;
        }
    }

    IEnumerator StartTalk()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, closePlayer.transform.position);
        if(distanceToPlayer <= range)
        {
            isTalking = true;
            text.SetActive(true);
            yield return new WaitForSeconds(3f);
            text.SetActive(false);
        }
    }

    IEnumerator EndTalk()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, closePlayer.transform.position);
        if (distanceToPlayer > range)
        {
            isTalking = false;
            text.SetActive(true);
            yield return new WaitForSeconds(3f);
            text.SetActive(false);
        }
    }
}
