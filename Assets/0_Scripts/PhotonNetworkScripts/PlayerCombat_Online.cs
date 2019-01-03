using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  Juan: añadidas las librerias de photon al duplicado del script de animación para poder 
/// </summary>

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

#region Componentes Requeridos
[RequireComponent(typeof(PlayerMovement_Online))]
[RequireComponent(typeof(PlayerHook_Online))]
#endregion

#region Playercombat Class
public class PlayerCombat_Online : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variables
        PlayerMovement_Online myPlayerMovement;
        PlayerWeapons myPlayerWeap;
        PlayerHook myHook;
        PlayerHUD myPlayerHUD;
        public float triggerDeadZone=0.15f;
        //List<string> attacks;
        [HideInInspector]
        public int attackIndex = 0;
        float chargingTime;
        float startupTime;
        float activeTime;
        float recoveryTime;
        public Material[] hitboxMats;
        [HideInInspector]
        public float knockBackSpeed = 30f;
        public Text attackName;
        [HideInInspector]
        public bool conHinchador = false;

        [HideInInspector]
        public List<string> targetsHit;

        [HideInInspector]
        public attackStage attackStg = attackStage.ready;
        public enum attackStage
        {
            ready=0,
            charging=1,
            startup=2,
            active=3,
            recovery=4
        }
        [HideInInspector]
        public List<AttackInfo_Online> myAttacks;//index: 0 = X; 1 = Y; 2 = B
    
        public Transform hitboxes;
        Collider hitbox;
        //public Collider hitbox;

        ///JUAN:
        ///Para el multiplayer son necesarias una serie de variables que nos permitan observar al jugador local y a los jugadores no locales
        ///para tratar esto usaremos un enum que dirá qué clase de ataque ha realizado el jugador
        
        public enum NetworkAttack
        {
            sendNothing,
            sendAttack1,
            sendAttack2,
            sendAttack3,
            sendHook,
            recievedNothing,
            recievedAttack1,
            recievedAttack2,
            recievedAttack3,
            recievedHook
        }
        [HideInInspector]
        public NetworkAttack attackSendSerialized = NetworkAttack.sendNothing;
        [HideInInspector]       
        public NetworkAttack attackRecievedSerialized = NetworkAttack.recievedNothing;

        [Tooltip("The local player Instance. Use this to know if the local player is represented in the current scene")]
        public static GameObject LocalPlayerInstance;
    #endregion

    #region Funciones de MonoBehaviour
        private void Awake()
        {
            PlayerCombat_Online.LocalPlayerInstance = this.gameObject;
            myPlayerMovement = GetComponent<PlayerMovement_Online>();
            myPlayerWeap = GetComponent<PlayerWeapons>();
            myHook = GetComponent<PlayerHook>();
            myPlayerHUD = myPlayerMovement.myPlayerHUD;
            attackStg = attackStage.ready;
            targetsHit = new List<string>();
            myAttacks = new List<AttackInfo_Online>();
        }

        private void Start()
        {
            if (photonView.IsMine)
            {
                Debug.Log("Juan: comentada la linea 106 de PlayerCombat_Online pues lanza la excepción: 'Object reference not set to an instance of an object PlayerCombat_Online.FillMyAttacks()'");
                //FillMyAttacks();
                attackIndex = -1;
                //ChangeAttackType(GameController.instance.attackX);
                HideAttackHitBox();
                //ChangeNextAttackType();
            }
            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                CalledOnLevelWasLoaded(scene.buildIndex);
            };
            #endif
        }

    void CalledOnLevelWasLoaded(int level)
    {
        // esto sirve para comprobar si el jugador ha cargado fuera de la "arena" y ponerlo en la posición original
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
    }

    public void KonoUpdate()
        {
            if (photonView.IsMine)
            {
                //print("Trigger = " + Input.GetAxis(myPlayerMovement.contName + "LT"));
                if (!myPlayerMovement.noInput && !myPlayerMovement.inWater && attackStg == attackStage.ready && !conHinchador)
                {
                    if (myPlayerMovement.Actions.Attack1.WasPressed && !myAttacks[0].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "X"))
                    {
                        ChangeAttackType(0);
                        StartAttack();
                        attackSendSerialized = NetworkAttack.sendAttack1; // Juan: para enviar que hemos hecho un ataque a otras personas presentes en la sala
                    }
                    if (myPlayerMovement.Actions.Attack2.WasPressed && !myAttacks[1].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "Y"))
                    {
                        ChangeAttackType(1);
                        StartAttack();
                        //ChangeNextAttackType();
                        attackSendSerialized = NetworkAttack.sendAttack2; // Juan: para enviar que hemos hecho un ataque a otras personas presentes en la sala
                    }
                    if (myPlayerMovement.Actions.Attack3.WasPressed && !myAttacks[2].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "B"))
                    {
                        ChangeAttackType(2);
                        StartAttack();
                        //ChangeNextAttackType();
                        attackSendSerialized = NetworkAttack.sendAttack3; // Juan: para enviar que hemos hecho un ataque a otras personas presentes en la sala
                    }
                    if (aiming && myPlayerMovement.Actions.Boost.WasPressed)// HOOK      //Input.GetButtonDown(myPlayerMovement.contName + "RB"))
                    {
                        myHook.StartHook();
                        //ChangeAttackType(GameController.instance.attackHook);
                        //StartAttack();
                        attackSendSerialized = NetworkAttack.sendHook; // Juan: para enviar que hemos lanzado el gancho a otras personas en la sala

                    }
                }

                ProcessAttack();
                ProcessAttacksCD();

                if (myPlayerMovement.Actions.Aim.WasPressed)
                {
                    StartAiming();
                }
                if (myPlayerMovement.Actions.Aim.WasReleased)
                {
                    StopAiming();
                }
            }
            else
            {
                switch (attackRecievedSerialized)
                {
                case NetworkAttack.recievedAttack1:
                    attackRecievedSerialized = NetworkAttack.recievedNothing;
                    ChangeAttackType(0);
                    StartAttack();
                    break;
                case NetworkAttack.recievedAttack2:
                    attackRecievedSerialized = NetworkAttack.recievedNothing;
                    ChangeAttackType(1);
                    StartAttack();
                    break;
                case NetworkAttack.recievedAttack3:
                    attackRecievedSerialized = NetworkAttack.recievedNothing;
                    ChangeAttackType(2);
                    StartAttack();
                    break;
                case NetworkAttack.recievedHook:
                    attackRecievedSerialized = NetworkAttack.recievedNothing;
                    myHook.StartHook();
                    break;
                }
            }
        }

    #endregion

    #region Functions
        public void FillMyAttacks()
        {
            AttackInfo_Online att = new AttackInfo_Online(GameController.instance.attackX);
            myAttacks.Add(att);
            att = new AttackInfo_Online(GameController.instance.attackY);
            myAttacks.Add(att);
            att = new AttackInfo_Online(GameController.instance.attackB);
            myAttacks.Add(att);
        }

        public void ChangeAttackType(int index)
        {
            attackIndex = index;
            AttackData attack = myAttacks[attackIndex].attack;
            attackName.text = attack.attackName;
            chargingTime = attack.chargingTime;
            startupTime = attack.startupTime;
            activeTime = attack.activeTime;
            recoveryTime = attack.recoveryTime;
            knockBackSpeed = attack.knockbackSpeed;
            //change hitbox
            if (hitboxes.childCount > 0)
            {
                for (int i = 0; i < hitboxes.childCount; i++)
                {
                    Destroy(hitboxes.GetChild(i).gameObject);
                }
            }

            GameObject newHitbox = Instantiate(attack.hitboxPrefab, hitboxes, false);
            hitbox = newHitbox.GetComponent<Collider>();
            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
        }

        public void HideAttackHitBox()
        {
            if (hitboxes.childCount > 0)
            {
                for (int i = 0; i < hitboxes.childCount; i++)
                {
                    Destroy(hitboxes.GetChild(i).gameObject);
                }
            }
        }

        /*void ChangeNextAttackType()
        {
                attackIndex++;
                if (attackIndex >= GameController.instance.allAttacks.Length)
                {
                    attackIndex = 0;
                }
                attackName.text = GameController.instance.allAttacks[attackIndex].attackName;
                chargingTime = GameController.instance.allAttacks[attackIndex].chargingTime;
                startupTime = GameController.instance.allAttacks[attackIndex].startupTime;
                activeTime = GameController.instance.allAttacks[attackIndex].activeTime;
                recoveryTime = GameController.instance.allAttacks[attackIndex].recoveryTime;
                knockBackSpeed = GameController.instance.allAttacks[attackIndex].knockbackSpeed;
                //change hitbox
                if (hitboxes.childCount > 0)
                {
                    for (int i = 0; i < hitboxes.childCount; i++)
                    {
                        Destroy(hitboxes.GetChild(i).gameObject);
                    }
                }

                GameObject newHitbox = Instantiate(GameController.instance.allAttacks[attackIndex].hitboxPrefab, hitboxes, false);
                hitbox = newHitbox.GetComponent<Collider>();
                hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
        }*/

        float attackTime = 0;
        public void StartAttack()
        {
            if (attackStg == attackStage.ready && !myPlayerMovement.noInput)
            {
                targetsHit.Clear();
                attackTime = 0;
                attackStg = chargingTime>0? attackStage.charging : attackStage.startup;
                hitbox.GetComponent<MeshRenderer>().material = hitboxMats[1];
            }
        }

        public void ProcessAttack()
        {
            if (attackStg != attackStage.ready)
            {
                attackTime += Time.deltaTime;
                switch (attackStg)
                {
                    case attackStage.ready:
                        break;
                    case attackStage.charging:
                        break;
                    case attackStage.startup:

                        //animacion startup
                        if (attackTime >= startupTime)
                        {
                            attackTime = 0;
                            attackStg = attackStage.active;
                            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[2];
                        }
                        break;
                    case attackStage.active:
                        if (attackTime >= activeTime)
                        {
                            attackTime = 0;
                            attackStg = attackStage.recovery;
                            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[3];
                        }
                        break;
                    case attackStage.recovery:
                        if (attackTime >= recoveryTime)
                        {
                            attackTime = 0;
                            attackStg = attackStage.ready;
                            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
                            HideAttackHitBox();

                            myAttacks[attackIndex].StartCD();

                        }
                        break;
                }
            }   
        }

        void ProcessAttacksCD()
        {
            for(int i = 0; i < myAttacks.Count; i++)
            {
                //Debug.LogWarning("Attack "+myAttacks[i].attack.attackName+" in cd? "+myAttacks[i].cdStarted);
               if(myAttacks[i].cdStarted)
                {
                    //print("Process CD attack + " + i);
                    myAttacks[i].ProcessCD();
                }
            }
        }

        [HideInInspector]
        public bool aiming;
        public void StartAiming()
        {
            if(!aiming)
            {
                aiming = true;
                myPlayerMovement.myCamera.SwitchCamera(CameraController.cameraMode.Shoulder);
                myPlayerWeap.AttachWeaponToBack();
                myPlayerHUD.StartAim();
                //ChangeAttackType(GameController.instance.attackHook);
            }  
        }

        public void StopAiming()
        {
            if (aiming)
            {
                aiming = false;
                myPlayerMovement.myCamera.SwitchCamera(CameraController.cameraMode.Free);
                myPlayerWeap.AttachWeapon();
                myPlayerHUD.StopAim();
            }
        }

    #endregion

    #region IPUNObservable imp

    ///para que funcione correctamente el combate en el online y procesar los datos de otros jugadores serializaremos una serie de booleanos (o en este caso un enum para enviarlos 
    ///a la cola de procesado del servidor así todos los usuarios conectados recibirán el cambio de cualquiera de los objetos jugador creados externos al jugador local
    ///intercambiando la mínima cantidad de datos posible
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
            ///The isWriting property will be true if this client is the "owner" of the PhotonView (and thus the GameObject).
            ///Add data to the stream and it's sent via the server to the other players in a room.
            ///On the receiving side, isWriting is false and the data should be read.

            //Juan: En tal caso en este lado del If manejaremos los datos que deseamos enviar a otros jugadores

                stream.SendNext(attackSendSerialized); // Juan: enviaremos siempre el enum de enviar
            }
        else
            {
                 //Juan: Y en este lado daremos valor a las cosas que recivimos
                this.attackRecievedSerialized = (NetworkAttack)stream.ReceiveNext(); // Juan: para recibir los datos siempre usaremos el enum de recibir.
            }
        }

    #endregion
}
#endregion

#region AttackInfo_Online class

public class AttackInfo_Online
{
    public AttackData attack;
    public float cdTime;
    public bool cdStarted;
    public AttackInfo_Online(AttackData _attack)
    {
        attack = _attack;
        cdTime = 0;
        cdStarted = false;
    }
    public void StartCD()
    {
        cdTime = 0;
        cdStarted = true;
        //Debug.Log("CD STARTED - ATTACK " + attack.attackName);
    }

    public void ProcessCD()
    {
        //Debug.Log("CD PROCESS - ATTACK " + attack.attackName + "; cdTime = " + cdTime);
        cdTime += Time.deltaTime;
        if (cdTime >= attack.cdTime)
        {
            StopCD();
        }
    }

    public void StopCD()
    {
        cdTime = 0;
        cdStarted = false;
        //Debug.Log("CD FINISHED - ATTACK " + attack.attackName);
    }
}

#endregion