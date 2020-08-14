using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectPlatform : MonoBehaviour
{
    public Transform rotationParent;//ROTATE THIS
    public GameObject TeamSelectionModels;
    public GameObject CharacterSelectionModels;
    public GameObject WeaponSelectionModels;

    PlayerModel[] charSelectPlayerModels;

    private void Awake()
    {
        charSelectPlayerModels = CharacterSelectionModels.GetComponentsInChildren<PlayerModel>();
    }

    public void StartTeamSelect()
    {
        TeamSelectionModels.SetActive(true);
        CharacterSelectionModels.SetActive(false);
        WeaponSelectionModels.SetActive(false);
    }
    public void StartCharacterSelection()
    {
        TeamSelectionModels.SetActive(false);
        CharacterSelectionModels.SetActive(true);
        WeaponSelectionModels.SetActive(false);
    }
    public void StartWeaponSelectionModels()
    {
        TeamSelectionModels.SetActive(false);
        CharacterSelectionModels.SetActive(false);
        WeaponSelectionModels.SetActive(true);
    }

    public void ChangeTeamColors(Team team)
    {
        for (int i = 0; i < charSelectPlayerModels.Length; i++)
        {
            charSelectPlayerModels[i].SwitchTeam(team);
        }
    }
}
