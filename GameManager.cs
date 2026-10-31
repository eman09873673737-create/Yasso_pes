
using UnityEngine;
using System.Collections.Generic;

public enum Formation
{
    FourThreeThree,
    FourFourTwo,
    FourTwoThreeOne,
    ThreeFiveTwo
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Match")]
    public float matchTime = 90f;
    public int homeScore;
    public int awayScore;
    public bool matchRunning = true;

    [Header("Teams")]
    public List<GameObject> playerTeam = new List<GameObject>();
    public List<GameObject> aiTeam = new List<GameObject>();

    public Rigidbody ball;
    public GameObject controlledPlayer;

    public Formation currentFormation = Formation.FourThreeThree;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (ball == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Ball");
            if (obj != null) ball = obj.GetComponent<Rigidbody>();
        }

        SetupFormation(currentFormation);

        if (playerTeam.Count > 0)
            SwitchControlTo(playerTeam[0]);
    }

    void Update()
    {
        if (!matchRunning) return;

        matchTime -= Time.deltaTime;

        if (matchTime <= 0)
        {
            matchTime = 0;
            matchRunning = false;
            Debug.Log("انتهت المباراة!");
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            SwitchToNearestPlayerToBall();
    }

    public void SwitchControlTo(GameObject newPlayer)
    {
        if (newPlayer == null) return;

        if (controlledPlayer != null)
        {
            PlayerController oldController =
                controlledPlayer.GetComponent<PlayerController>();

            PlayerAI oldAI =
                controlledPlayer.GetComponent<PlayerAI>();

            if (oldController != null) oldController.enabled = false;
            if (oldAI != null) oldAI.enabled = true;
        }

        controlledPlayer = newPlayer;

        PlayerController controller =
            newPlayer.GetComponent<PlayerController>();

        PlayerAI ai = newPlayer.GetComponent<PlayerAI>();

        if (controller != null) controller.enabled = true;
        if (ai != null) ai.enabled = false;
    }

    public void SwitchToNearestPlayerToBall()
    {
        if (ball == null) return;

        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject p in playerTeam)
        {
            if (p == null) continue;

            float distance = Vector3.Distance(
                p.transform.position, ball.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = p;
            }
        }

        if (nearest != null)
            SwitchControlTo(nearest);
    }

    public GameObject GetNearestTeammate(GameObject passer)
    {
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject p in playerTeam)
        {
            if (p == null || p == passer) continue;

            float distance = Vector3.Distance(
                p.transform.position, passer.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = p;
            }
        }

        return nearest;
    }

    public void SetupFormation(Formation formation)
    {
        currentFormation = formation;

        float[] xPositions;
        float[] zPositions;

        switch (formation)
        {
            case Formation.FourFourTwo:
                xPositions = new float[]
                { 0,-20,-7,7,20,-21,-7,7,21,-9,9 };
                zPositions = new float[]
                { -46,-30,-32,-32,-30,-10,-10,-10,-10,15,15 };
                break;

            case Formation.FourTwoThreeOne:
                xPositions = new float[]
                { 0,-20,-7,7,20,-8,8,-18,0,18,0 };
                zPositions = new float[]
                { -46,-30,-32,-32,-30,-14,-14,4,5,4,20 };
                break;

            case Formation.ThreeFiveTwo:
                xPositions = new float[]
                { 0,-16,0,16,-22,-10,0,10,22,-8,8 };
                zPositions = new float[]
                { -46,-32,-34,-32,-12,-10,-10,-10,-12,15,15 };
                break;

            default:
                xPositions = new float[]
                { 0,-20,-7,7,20,-12,0,12,-18,0,18 };
                zPositions = new float[]
                { -46,-30,-32,-32,-30,-10,-12,-10,15,18,15 };
                break;
        }

        for (int i = 0; i < playerTeam.Count && i < 11; i++)
        {
            if (playerTeam[i] == null) continue;

            Vector3 pos = new Vector3(
                xPositions[i],
                playerTeam[i].transform.position.y,
                zPositions[i]);

            playerTeam[i].transform.position = pos;

            PlayerAI ai = playerTeam[i].GetComponent<PlayerAI>();
            if (ai != null) ai.homePosition = pos;
        }

        Debug.Log("تم تطبيق الخطة: " + formation);
    }

    public void Goal(bool homeTeamScored)
    {
        if (homeTeamScored) homeScore++;
        else awayScore++;

        Debug.Log("النتيجة: " + homeScore + " - " + awayScore);

        if (ball != null)
        {
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
            ball.position = Vector3.up * 0.5f;
        }
    }

    public void MakeSubstitution(int outIndex, int inIndex)
    {
        if (outIndex < 0 || outIndex >= playerTeam.Count ||
            inIndex < 0 || inIndex >= playerTeam.Count)
            return;

        GameObject outgoing = playerTeam[outIndex];
        GameObject incoming = playerTeam[inIndex];

        if (outgoing == null || incoming == null) return;

        Vector3 temp = outgoing.transform.position;
        outgoing.transform.position = incoming.transform.position;
        incoming.transform.position = temp;

        playerTeam[outIndex] = incoming;
        playerTeam[inIndex] = outgoing;

        if (controlledPlayer == outgoing)
            SwitchControlTo(incoming);

        Debug.Log("تم إجراء التبديل.");
    }
}
