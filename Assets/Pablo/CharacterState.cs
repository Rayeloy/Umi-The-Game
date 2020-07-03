using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AuthCharPredictor : MonoBehaviour, IAuthCharStateHandler
{
    LinkedList<CharacterInput> pendingInputs;
    PlayerMovementCMF character;
    CharacterState predictedState;
    private CharacterState lastServerState = CharacterState.Zero;

    void Awake()
    {
        pendingInputs = new LinkedList<CharacterInput>();
        character = GetComponent<PlayerMovementCMF>();
    }

    public void AddInput(CharacterInput input)
    {
        pendingInputs.AddLast(input);
        ApplyInput(input);
        character.SyncState(predictedState);
    }

    public void OnStateChange(CharacterState newState)
    {
        if (newState.timestamp <= lastServerState.timestamp) return;
        while (pendingInputs.Count > 0 && pendingInputs.First().inputNum <= newState.moveNum)
        {
            pendingInputs.RemoveFirst();
        }
        predictedState = newState;
        lastServerState = newState;
        UpdatePredictedState();
    }

    void UpdatePredictedState()
    {
        foreach (CharacterInput input in pendingInputs)
        {
            ApplyInput(input);
        }
        character.SyncState(predictedState);
    }

    void ApplyInput(CharacterInput input)
    {
        predictedState = CharacterState.Move(predictedState, input, 0, character);
    }

}

[System.Serializable]
public struct CharacterState
{
    public Vector3 position;
    public bool jumpState;
    public Vector3 eulerAngles;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public int moveNum;
    public int timestamp;

    public override string ToString()
    {
        return string.Format("CharacterState Pos:{0}|Rot:{1}|Vel:{2}|AngVel:{3}|MoveNum:{4}|Timestamp:{5}", position, eulerAngles, velocity, angularVelocity, moveNum, timestamp);
    }

    public static CharacterState Zero
    {
        get
        {
            return new CharacterState
            {
                position = Vector3.zero,
                eulerAngles = Vector3.zero,
                moveNum = 0,
                timestamp = 0
            };
        }
    }

    public static CharacterState Interpolate(CharacterState from, CharacterState to, int clientTick)
    {
        float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);
        return new CharacterState
        {
            position = Vector3.Lerp(from.position, to.position, t),
            eulerAngles = Vector3.Lerp(from.eulerAngles, to.eulerAngles, t),
            moveNum = 0,
            timestamp = 0
        };
    }

    public static CharacterState Extrapolate(CharacterState from, int clientTick)
    {
        int t = clientTick - from.timestamp;
        return new CharacterState
        {
            position = from.position + from.velocity * t,
            eulerAngles = from.eulerAngles + from.eulerAngles * t,
            moveNum = from.moveNum,
            timestamp = from.timestamp
        };
    }

    public static CharacterState Move(CharacterState previous, CharacterInput input, int timestamp, PlayerMovementCMF player)
    {
        return player.StateOnlineMover(timestamp);
    }
}

public struct CharacterInput
{
    public CharacterInput(Vector2 _dir, int _inputNum)
    {
        dir = _dir;
        inputNum = _inputNum;
    }
    public Vector2 dir;
    public int inputNum;
}

public class AuthCharServer : MonoBehaviour
{
    Queue<CharacterInput> inputBuffer;
    PlayerMovementCMF character;
    int movesMade;
    int serverTick;

    //CharacterController charCtrl;

    void Awake()
    {
        inputBuffer = new Queue<CharacterInput>();
        character = GetComponent<PlayerMovementCMF>();
        character.state = CharacterState.Zero;
        //charCtrl = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (movesMade > 0)
            movesMade--;
        if (movesMade == 0)
        {
            CharacterState state = character.state;
            while ((movesMade < character.InputBufferSize && inputBuffer.Count > 0))
            {
                state = CharacterState.Move(state, inputBuffer.Dequeue(), serverTick,character);
                //charCtrl.Move(state.position - charCtrl.transform.position);
                movesMade++;
            }
            if (movesMade > 0)
            {
                state.position = transform.position;
                character.OnServerStateChange(character.state, state);
                character.state = state;
            }
        }
    }

    void FixedUpdate()
    {
        serverTick++;
    }

    public void Move(CharacterInput[] inputs)
    {
        foreach (var input in inputs)
            inputBuffer.Enqueue(input);
    }
}

public interface IAuthCharStateHandler
{
    void OnStateChange(CharacterState newState);
}

public class AuthCharInput : MonoBehaviour
{
    public static bool simulated = false;

    List<CharacterInput> inputBuffer;
    PlayerMovementCMF character;
    AuthCharPredictor predictor;
    int currentInput = 0;
    Vector2 simVector;

    void Awake()
    {
        inputBuffer = new List<CharacterInput>();
        character = GetComponent<PlayerMovementCMF>();
        predictor = GetComponent<AuthCharPredictor>();
        if (simulated)
        {
            simVector.x = Random.Range(0, 1) > 0 ? 1 : -1;
            simVector.y = Random.Range(-1f, 1f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            simulated = !simulated;
        Vector2 input = simulated ? SimulatedVector() : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputBuffer.Count == 0 && input == Vector2.zero)
            return;
        CharacterInput charInput = new CharacterInput(input, currentInput++);
        predictor.AddInput(charInput);
        inputBuffer.Add(charInput);
    }

    void FixedUpdate()
    {
        if (inputBuffer.Count < character.InputBufferSize)
            return;
        character.CmdMove(inputBuffer.ToArray());
        inputBuffer.Clear();
    }


    Vector2 SimulatedVector()
    {
        if (transform.position.x > 5)
            simVector.x = Random.Range(-1f, 0);
        else if (transform.position.x < -5)
            simVector.x = Random.Range(0, 1f);
        if (transform.position.z > 2 || transform.position.z < -2)
            simVector.y = 0;
        return simVector;
    }
}


