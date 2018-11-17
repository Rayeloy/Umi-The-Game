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


	[Header("Referencias")]
	public TextMeshProUGUI[] blueTeamScore_Text;
	public TextMeshProUGUI[] redTeamScore_Text;
	public TextMeshProUGUI[] time_Text;

    public void TiempoDeJuego (){
		if(End) return;

		Tiempo -= Time.deltaTime;

		string elTiempo;
		//Minutos
		if (Tiempo/60 < 10)
			elTiempo = "0" + Mathf.FloorToInt(Tiempo/60).ToString();
		else
			elTiempo = Mathf.FloorToInt(Tiempo/60).ToString();

		elTiempo = elTiempo + ":";
		//Segundos
		if (Tiempo%60 < 10)
			elTiempo = elTiempo + "0" + Mathf.FloorToInt(Tiempo%60).ToString();
		else
			elTiempo = elTiempo + Mathf.FloorToInt(Tiempo%60).ToString();

		foreach(TextMeshProUGUI tM in time_Text)
			tM.text = elTiempo;

		Debug.Log(elTiempo);

        if (Tiempo <= 0){
            Team winner;
            if (_blueTeamScore == _redTeamScore)
            {
                //PROGRAMAR EL CASO DE QUE AMBOS ACABEN CON EL MISMO SCORE
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
                foreach (TextMeshProUGUI tM in blueTeamScore_Text)
                {
                    tM.text = _blueTeamScore.ToString();
                }
                if (_blueTeamScore >= maxScore)
                {
                    GameController.instance.GameOver(scoringTeam);
                }
                break;
            case Team.red:
                _redTeamScore++;
                foreach (TextMeshProUGUI tM in redTeamScore_Text)
                {
                    tM.text = _redTeamScore.ToString();
                }
                if (_redTeamScore >= maxScore)
                {
                    GameController.instance.GameOver(scoringTeam);
                }
                break;
        }
	}
}
