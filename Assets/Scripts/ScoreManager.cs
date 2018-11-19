using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager instance;
	// Use this for initialization
    private void Awake()
    {
        instance = this;
    }

    [Tooltip("Teimpo de juego en segundos")]
    public float Tiempo = 120;
    private float _Tiempo;

	[HideInInspector]
	public bool End = false;

    public int maxScore;
    public int _blueTeamScore;
	public int _redTeamScore;

    public float tiempoProrroga = 0.0f;
    private float _tiempoProrroga;
    [HideInInspector]
    public bool prorroga = false;

    public GameObject[] orcasRedTeam;
    private List<GameObject> _orcasRedTeam;
    public GameObject [] orcasBlueTeam;
    private List<GameObject> _orcasBlueTeam;

	[Header("Referencias")]
	public TextMeshProUGUI[] blueTeamScore_Text;
	public TextMeshProUGUI[] redTeamScore_Text;
	public TextMeshProUGUI[] time_Text;

    public void KonoStart(){
        _Tiempo = tiempoProrroga;
        _tiempoProrroga = tiempoProrroga;

        for( int i = 0; i < orcasRedTeam.Length; i++){
            _orcasRedTeam.Add(orcasRedTeam[i]);
        }
        for( int i = 0; i < orcasBlueTeam.Length; i++){
            _orcasBlueTeam.Add(orcasBlueTeam[i]);
        }
    }

    public void Reset(){
        prorroga = false;
        End = false;

        //Tiempos
        _Tiempo = tiempoProrroga;
        _tiempoProrroga = tiempoProrroga;
        for( int i = 0; i < time_Text.Length; i++)
			time_Text[i].color = Color.white;

        //Scores
        _blueTeamScore = 0;
        for( int i = 0; i < blueTeamScore_Text.Length; i++){
            blueTeamScore_Text[i].text = _blueTeamScore.ToString();
        }

        _redTeamScore = 0;
        for( int i = 0; i < redTeamScore_Text.Length; i++){
            redTeamScore_Text[i].text = _redTeamScore.ToString();
        }
    }

    public void KonoUpdate (){
		if(End) return;

        if (prorroga)
        {
            Prorroga();
            return;
        }

		_Tiempo -= Time.deltaTime;

        for( int i = 0; i < time_Text.Length; i++)
			time_Text[i].text = timeToString(_Tiempo);

        if (_Tiempo <= 0){
            Team winner;
            if (_blueTeamScore == _redTeamScore)
            {
                //PROGRAMAR EL CASO DE QUE AMBOS ACABEN CON EL MISMO SCORE
                SetProrroga();
            }
            else
            {
                winner = _blueTeamScore > _redTeamScore ? Team.blue : Team.red;
                GameController.instance.GameOver(winner);
            }
        }
    }

	public void ScorePoint (Team scoringTeam){
        switch (scoringTeam)
        {
            case Team.blue:
                _blueTeamScore++;
                RandomOrcaSpawn(_orcasBlueTeam);
                for( int i = 0; i < blueTeamScore_Text.Length; i++)
                {
                    blueTeamScore_Text[i].text = _blueTeamScore.ToString();
                }
                if (_blueTeamScore >= maxScore || prorroga)
                {
                    GameController.instance.GameOver(scoringTeam);
                }
                break;
            case Team.red:
                _redTeamScore++;
                RandomOrcaSpawn(_orcasRedTeam);
                for( int i = 0; i < redTeamScore_Text.Length; i++)//foreach (TextMeshProUGUI tM in redTeamScore_Text)
                {
                    redTeamScore_Text[i].text = _redTeamScore.ToString();
                }
                if (_redTeamScore >= maxScore || prorroga)
                {
                    GameController.instance.GameOver(scoringTeam);
                }
                break;
        }
	}

    private void RandomOrcaSpawn (List<GameObject> l){
        if (l.Count < 0) return;

        int i = Random.Range(0, l.Count - 1);
        l[i].SetActive( true );

        l.RemoveAt(i);
    }

    private string timeToString(float f){
        string elTiempo = "";

        //Minutos
		if (f/60 < 10)
            elTiempo = "0";

		elTiempo = elTiempo + Mathf.FloorToInt(f/60).ToString() + ":";
		//Segundos
		if (f%60 < 10)
            elTiempo = elTiempo + "0";
        
        elTiempo = elTiempo + Mathf.FloorToInt(f%60).ToString();

        return elTiempo;
    }

#region Prorroga

    private void SetProrroga(){
        prorroga = true;

        for( int i = 0; i < time_Text.Length; i++)
			time_Text[i].color = Color.red;
    }

    private void Prorroga(){
        _tiempoProrroga -= Time.deltaTime;

        if (_tiempoProrroga <= 0){
            _tiempoProrroga = 0;
            GameController.instance.GameOver(Team.none);
            End = true;
        }

        for( int i = 0; i < time_Text.Length; i++)
			time_Text[i].text = timeToString(_tiempoProrroga);
    }

    private int nPlayerEliminados = 0;
    public void PlayerEliminado ()
    {
        if (prorroga){
            nPlayerEliminados++;

            if (nPlayerEliminados >= GameController.instance.playerNum){
                End = true;
                GameController.instance.GameOver(Team.none);
            }
        }
    }

#endregion
}
