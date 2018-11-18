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

	[HideInInspector]
	public bool End = false;

    public int maxScore;
    public int _blueTeamScore;
	public int _redTeamScore;

    public float tiempoProrroga = 0.0f;
    [HideInInspector]
    public bool prorroga = false;

	[Header("Referencias")]
	public TextMeshProUGUI[] blueTeamScore_Text;
	public TextMeshProUGUI[] redTeamScore_Text;
	public TextMeshProUGUI[] time_Text;

    public void TiempoDeJuego (){
		if(End) return;

        if (prorroga)
        {
            Prorroga();
            return;
        }

		Tiempo -= Time.deltaTime;

		//string elTiempo = "" ;
		////Minutos
		//if (Tiempo/60 < 10) elTiempo = "0";

		//elTiempo = elTiempo + Mathf.FloorToInt(Tiempo/60).ToString() + ":";
		////Segundos
		//if (Tiempo%60 < 10) elTiempo = elTiempo + "0";
        //
        //elTiempo = elTiempo + Mathf.FloorToInt(Tiempo%60).ToString();

        for( int i = 0; i < time_Text.Length; i++)
			time_Text[i].text = timeToString(Tiempo);

        if (Tiempo <= 0){
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

    private string timeToString(float f){
        string elTiempo = "";

        //Minutos
		if (Tiempo/60 < 10)
            elTiempo = "0";

		elTiempo = elTiempo + Mathf.FloorToInt(Tiempo/60).ToString() + ":";
		//Segundos
		if (Tiempo%60 < 10)
            elTiempo = elTiempo + "0";
        
        elTiempo = elTiempo + Mathf.FloorToInt(Tiempo%60).ToString();

        return elTiempo;
    }

#region Prorroga

    private void SetProrroga(){
        prorroga = true;

        for (int i = 0; i < GameController.instance.allPlayers.Length; i++){

        }
    }

    private void Prorroga(){
        tiempoProrroga -= Time.deltaTime;

        if (tiempoProrroga <= 0){
            GameController.instance.GameOver(Team.none);
        }
    }

    private int nPlayerEliminados = 0;
    public void PlayerEliminado ()
    {
        nPlayerEliminados++;

        if (nPlayerEliminados >= GameController.instance.playerNum){
            GameController.instance.GameOver(Team.none);
        }
    }

#endregion
}
