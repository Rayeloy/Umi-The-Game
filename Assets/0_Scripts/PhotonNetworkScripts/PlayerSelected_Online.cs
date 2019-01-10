using UnityEngine;

public class PlayerSelected_Online : MonoBehaviour
{
    #region Variables
    public PlayerActions Actions { get; set; }

    public Team_Online team = Team_Online.none;

    public SkinnedMeshRenderer Body;
    public Material teamNeutralMat;
    public Material teamBlueMat;
    public Material teamRedMat;

    //public Renderer cachedRenderer;

    private bool _ready = false;
    /// <value>The Name property gets/sets the value of the string field, _name.</value>
    public bool isReady
    {
        get { return _ready; }
        set
        {
            animator.SetBool("Ready", value);
            _ready = value;
        }
    }

    [Header("Referencias")]
    [HideInInspector]
    public PlayerSelecionUI playerSelecionUI;
    public Sprite PlayerSelectRandom;
    public Sprite PlayerSelectBlue;
    public Sprite PlayerSelectRed;
    public Sprite PlayerSelectedRandom;
    public Sprite PlayerSelectedBlue;
    public Sprite PlayerSelectedRed;

    //public bool Ready = false;
    [Header("Referencias")]
    public Animator animator;
    [HideInInspector]
    public bool isAReleased = true;
    #endregion

    #region MonoBehaviourCallbacks
    void Update()
    {
        if (Actions.Jump.WasPressed && isAReleased)
            SetReady ();
    }
    #endregion

    #region Private Functions

    public void changeTeam(Team_Online t)
    {
        team = t;
        switch (t)
        {
            case Team_Online.blue:
                Body.material = teamBlueMat;
                playerSelecionUI.TeamSelect.sprite = PlayerSelectBlue;
                break;
            case Team_Online.red:
                Body.material = teamRedMat;
                playerSelecionUI.TeamSelect.sprite = PlayerSelectRed;
                break;
            case Team_Online.none:
                Body.material = teamNeutralMat;
                playerSelecionUI.TeamSelect.sprite = PlayerSelectRandom;
                break;
        }
    }

    private void SetReady ()
    {
        isReady = !isReady;

        playerSelecionUI.FlechaIzquierda.enabled = !playerSelecionUI.FlechaIzquierda.enabled;
        playerSelecionUI.FlechaDerecha.enabled = !playerSelecionUI.FlechaDerecha.enabled;

        if (isReady)
        {
            playerSelecionUI.AcctionsText.text = "B to back";
            switch (team)
            {
                case Team_Online.blue:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectedBlue;
                    break;
                case Team_Online.red:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectedRed;
                    break;
                case Team_Online.none:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectedRandom;
                    break;
            }
        }
        else
        {
            playerSelecionUI.AcctionsText.text = "Press to choose";
            switch (team)
            {
                case Team_Online.blue:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectBlue;
                    break;
                case Team_Online.red:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectRed;
                    break;
                case Team_Online.none:
                    playerSelecionUI.TeamSelect.sprite = PlayerSelectRandom;
                    break;
            }
        }
    }

    #endregion
}
