using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [Tooltip("Teimpo de juego en segundos")]
    public float Tiempo = 120;

	[HideInInspector]
	public bool End = false;

	public int _blueTeamScore;
	public int _redTeamScore;

	[Header("Referencias")]
	public GameController gameController;
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
            gameController.playing = false;

			if (_blueTeamScore > _redTeamScore)
				gameController.winnerTeam = Team.blue;
			else
				gameController.winnerTeam = Team.red;
			gameController.SwitchGameOverMenu();
        }
    }

	public void ScorePoint (Team _winnerTeam){
		switch (_winnerTeam)
        {
            case Team.blue :
				_blueTeamScore ++;
				foreach(TextMeshProUGUI tM in blueTeamScore_Text)
					tM.text = _blueTeamScore.ToString();
                break;
            case Team.red :
				_redTeamScore ++;
				foreach(TextMeshProUGUI tM in redTeamScore_Text)
					tM.text = _redTeamScore.ToString();
                break;
        }
	}
}
