using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManagerCMF : MonoBehaviour
{
    public GameControllerCMF_FlagMode gC;
    // Use this for initialization

    [Tooltip("Tiempo de juego en segundos")]
    public float gameTime = 120;
    private float currentGameTime=0;

    [HideInInspector]
    public bool End = false;


    public int maxScore;
    private int _teamAScore;
    private int _teamBScore;

    [SerializeField]
    private float tiempoProrroga = 0.0f;
    private float _tiempoProrroga;
    [HideInInspector]
    public bool prorroga = false;

    public GameObject[] teamAWhales;
    List<int> orcasRedIndex;
    public GameObject[] teamBWhales;
    List<int> orcasBlueIndex;

    [Header("Referencias")]
    public ParticleSystem teamAFireworks;
    public ParticleSystem teamBFireworks;
    [HideInInspector]
    public List<TextMeshProUGUI> teamAScore_Text;
    [HideInInspector]
    public List<TextMeshProUGUI> teamBScore_Text;
    [HideInInspector]
    public List<TextMeshProUGUI> time_Text;
    //public RectTransform[] contador;


    public void KonoAwake(GameControllerCMF_FlagMode _gC)
    {
        gC = _gC;
        teamAScore_Text = new List<TextMeshProUGUI>();
        teamBScore_Text = new List<TextMeshProUGUI>();
        time_Text = new List<TextMeshProUGUI>();
        currentGameTime = gameTime;
        _tiempoProrroga = tiempoProrroga;
        orcasBlueIndex = new List<int>();
        orcasRedIndex = new List<int>();
    }

    public void KonoStart()
    {

        for (int i = 0; i < teamAWhales.Length; i++)
        {
            teamAWhales[i].SetActive(false);
            orcasRedIndex.Add(i);
        }
        for (int i = 0; i < teamBWhales.Length; i++)
        {
            teamBWhales[i].SetActive(false);
            orcasBlueIndex.Add(i);
        }
    }

    public void Reset()
    {
        prorroga = false;
        End = false;

        //Tiempos
        currentGameTime = gameTime;
        _tiempoProrroga = tiempoProrroga;
        for (int i = 0; i < time_Text.Count; i++)
            time_Text[i].color = Color.white;

        //Scores
        _teamAScore = 0;
        for (int i = 0; i < teamAScore_Text.Count; i++)
        {
            teamAScore_Text[i].text = _teamAScore.ToString();
        }

        _teamBScore = 0;
        for (int i = 0; i < teamBScore_Text.Count; i++)
        {
            teamBScore_Text[i].text = _teamBScore.ToString();
        }

        nPlayerEliminados = 0;

        ResetOrcas();
    }

    public void KonoUpdate()
    {
        if (End) return;

        if (prorroga)
        {
            Prorroga();
            return;
        }

        currentGameTime -= Time.deltaTime;
        for (int i = 0; i < time_Text.Count; i++)
        {
            time_Text[i].text = timeToString(currentGameTime);
        }


        if (currentGameTime <= 0)
        {
            Team winner;
            if (_teamAScore == _teamBScore)
            {
                //PROGRAMAR EL CASO DE QUE AMBOS ACABEN CON EL MISMO SCORE
                SetProrroga();
            }
            else
            {
                winner = _teamAScore > _teamBScore ? Team.A : Team.B;
                gC.StartGameOver(winner);
            }
        }
    }

    public void ScorePoint(Team scoringTeam)
    {
        switch (scoringTeam)
        {
            case Team.A:
                _teamAScore++;
                RandomOrcaSpawn(Team.A);
                if (teamAFireworks != null)
                    teamAFireworks.Play(true);
                for (int i = 0; i < teamAScore_Text.Count; i++)
                {
                    teamAScore_Text[i].text = _teamAScore.ToString();
                }
                if (_teamAScore >= maxScore || prorroga)
                {
                    gC.StartGameOver(scoringTeam);
                }
                break;
            case Team.B:
                _teamBScore++;
                RandomOrcaSpawn(Team.B);
                if (teamBFireworks != null)
                    teamBFireworks.Play(true);
                for (int i = 0; i < teamBScore_Text.Count; i++)//foreach (TextMeshProUGUI tM in teamBScore_Text)
                {
                    teamBScore_Text[i].text = _teamBScore.ToString();
                }
                if (_teamBScore >= maxScore || prorroga)
                {
                    gC.StartGameOver(scoringTeam);
                }
                break;
        }
    }

    private void RandomOrcaSpawn(Team team)
    {
        switch (team)
        {
            case Team.A:
                if (orcasBlueIndex.Count == 0)
                {
                    return;
                }
                int i = Random.Range(0, orcasBlueIndex.Count - 1);
                int index = orcasBlueIndex[i];
                teamAWhales[index].SetActive(true);
                orcasBlueIndex.RemoveAt(i);
                break;
            case Team.B:
                if (orcasRedIndex.Count == 0)
                {
                    return;
                }
                i = Random.Range(0, orcasRedIndex.Count - 1);
                index = orcasRedIndex[i];
                teamBWhales[index].SetActive(true);
                orcasRedIndex.RemoveAt(i);
                break;
        }
    }

    void ResetOrcas()
    {
        orcasBlueIndex = new List<int>();
        orcasRedIndex = new List<int>();

        for (int i = 0; i < teamBWhales.Length; i++)
        {
            teamBWhales[i].SetActive(false);
            orcasRedIndex.Add(i);
        }
        for (int i = 0; i < teamAWhales.Length; i++)
        {
            teamAWhales[i].SetActive(false);
            orcasBlueIndex.Add(i);
        }
    }

    private string timeToString(float f)
    {
        string elTiempo = "";

        //Minutos
        if (f / 60 < 10)
            elTiempo = "0";

        elTiempo = elTiempo + Mathf.FloorToInt(f / 60).ToString() + ":";
        //Segundos
        if (f % 60 < 10)
            elTiempo = elTiempo + "0";

        elTiempo = elTiempo + Mathf.FloorToInt(f % 60).ToString();

        return elTiempo;
    }

    #region Prorroga

    private void SetProrroga()
    {
        prorroga = true;

        for (int i = 0; i < time_Text.Count; i++)
            time_Text[i].color = Color.red;
    }

    private void Prorroga()
    {
        _tiempoProrroga -= Time.deltaTime;

        if (_tiempoProrroga <= 0)
        {
            _tiempoProrroga = 0;
            gC.StartGameOver(Team.none);
            End = true;
        }

        for (int i = 0; i < time_Text.Count; i++)
            time_Text[i].text = timeToString(_tiempoProrroga);
    }

    private int nPlayerEliminados = 0;
    public void PlayerEliminado()
    {
        if (prorroga)
        {
            nPlayerEliminados++;

            Debug.Log(nPlayerEliminados + " " + gC.playerNum);
            if (nPlayerEliminados >= gC.playerNum)
            {
                End = true;
                gC.StartGameOver(Team.none);
            }
        }
    }

    #endregion
}
