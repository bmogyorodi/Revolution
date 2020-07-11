using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataController :MonoBehaviour
{
    public Building[] buildings;
    public Character[] characters;
    private int selectedSwap = 0;
    

    public int ReturnBuildingIndex(string name)
    {
        for(int i=0; i<buildings.Length; i++)
        {
            if (buildings[i].buildingName == name)
            {
                return i;
            }
        }
        return -1;
    }
    public int ReturnCharacterIndex(string name)
    {
        for(int i=0; i<characters.Length; i++)
        {
            if (characters[i].buildingName == name)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsBoardFilled()
    {
        bool full = false;
        for (int i = 0; i < buildings.Length; i++)
        {
            full = buildings[i].IsBuildingFull();
            if (!full) { return false; }
        }
        return full;
    }
    public void SelectBuildingSlot(int buildingIndex, int blockNumber,Button button,Text buildingof)
    {
        buildings[buildingIndex].SelectBuildingSlot(blockNumber,button,buildingof);
    }
    public void SelectBuildingSlot(int buildingIndex, int blockNumber, Button[] buttons, Text[] buildingof)
    {
        buildings[buildingIndex].SelectBuildingSlot(blockNumber, buttons, buildingof,this);
    }

    public void SlotButtonSetting(int i, int j,Button button,Text buildingof)
    {
        buildings[i].occupancy[j].onClick.AddListener(() => SelectBuildingSlot(i, j,button,buildingof));
    }

    public void SwapSlotSetting(int i, int j, Button[] buttons, Text[] textsofButtons, DataController data)
    {

        buildings[i].occupancy[j].onClick.AddListener(() => SelectBuildingSlot(i, j, buttons, textsofButtons));

    }
    public void ExecuteSpy(Button button, Text buildingof,Player player,GameObject panel)
    {
        if (CheckifColored(button,buildingof))
        {
            Debug.Log("No building or player selected for attack!");
        }
        else
        {
            string buildingtarget = buildingof.text;
            int buildingIndex = ReturnBuildingIndex(buildingtarget);
            if (buildingIndex == -1)
            {
                Debug.LogError("Building not found!");
            }
            
                int occupancyIndex = FirstOccupancy(button.image.color, buildingIndex);
                buildings[buildingIndex].occupancy[occupancyIndex].image.color = player.playerColor;
            ResetIcon(button, buildingof);
            panel.SetActive(false);

                
               
                
            
        }
    }
    public void ExecuteSpy(Player me, Player target,int buildingIndex)
    {
        Debug.Log("Spy executor"+buildingIndex);
        int occupancyIndex = FirstOccupancy(target.playerColor, buildingIndex);
        buildings[buildingIndex].occupancy[occupancyIndex].image.color = me.playerColor;
    }
    public void ExecuteApothecary(Button[] buttons,Text[] buildingsof,DisplayManager boardfreezer,Button confirmer,Button spyButton,GameObject panel)
    {
        if (buttons.Length!=2 || buildingsof.Length != 2)
        {
            Debug.LogError("Wrong Button or text array size for switch. Would need 2!");

        }
        else
        {
            if(CheckifColored(buttons[0],buildingsof[0]) || CheckifColored(buttons[1], buildingsof[1]))
            {
                Debug.Log("One or more spaces were not selected for the swap!");
            }
            else
            {
                string buildingtarget1 = buildingsof[0].text;
                int buildingIndex1 = ReturnBuildingIndex(buildingtarget1);
                string buildingtarget2 = buildingsof[1].text;
                int buildingIndex2 = ReturnBuildingIndex(buildingtarget2);
                if(buildingIndex1==-1 || buildingIndex2 == -1)
                {
                    Debug.LogError("Building not found!");
                }
                int occupancyIndex1 = FirstOccupancy(buttons[0].image.color, buildingIndex1);
                int occupancyIndex2 = FirstOccupancy(buttons[1].image.color, buildingIndex2);
                Color heldcolor = buildings[buildingIndex1].occupancy[occupancyIndex1].image.color;
                buildings[buildingIndex1].occupancy[occupancyIndex1].image.color = buildings[buildingIndex2].occupancy[occupancyIndex2].image.color;
                buildings[buildingIndex2].occupancy[occupancyIndex2].image.color = heldcolor;
                ResetIcon(buttons[0], buildingsof[0]);
                ResetIcon(buttons[1], buildingsof[1]);
                confirmer.interactable = false;
                panel.SetActive(false);
               
                SwitchSwap(0);
                
            }
        }
    }
    public void ExecuteApothecary(Player me,Player target,int mybuilding,int targetbuilding)
    {
        int occupancyIndexme = FirstOccupancy(me.playerColor, mybuilding);
        int occupancyIndextarget = FirstOccupancy(target.playerColor, targetbuilding);
        Color heldcolor = buildings[mybuilding].occupancy[occupancyIndexme].image.color;
        buildings[mybuilding].occupancy[occupancyIndexme].image.color = buildings[targetbuilding].occupancy[occupancyIndextarget].image.color;
        buildings[targetbuilding].occupancy[occupancyIndextarget].image.color = heldcolor;
    }
   
    private bool CheckifColored(Button button,Text text)
    {
        return (text.text == "" || button.image.color == new Color(255, 255, 255));
    }
    private void ResetIcon(Button button,Text text)
    {
        button.image.color = new Color(255, 255, 255);
        text.text = "";
        button.interactable = false;
    }

    private int FirstOccupancy(Color playercolor,int buildingIndex)
    {
        
       for(int i=0; i< buildings[buildingIndex].occupancy.Length; i++)
        {
            if(buildings[buildingIndex].occupancy[i].image.color==playercolor)
            {
                return i;
            }
        }
        return -1;

    }


   
    
    public void SwitchSwap(int oneornull)
    {
        selectedSwap = oneornull;
    }
    public int GetSwitch()
    { return selectedSwap; }

    public void SetBuildingOwners(Player[] players)
    {
        for(int i=0; i<buildings.Length; i++)
        {
            buildings[i].FindOwner(players);
        }
    }

    public void GivePointsToOwners(Player[] players)
    {
        for(int i=0; i<buildings.Length; i++)
        {
            if (buildings[i].GetHasOwner())
            {
                Debug.Log("Winner: " + buildings[i].GetOwnerIndex()+" gets "+ buildings[i].pointsforOwner); 
                players[buildings[i].GetOwnerIndex()].AddPoints(buildings[i].pointsforOwner);
            }
        }
    }
    public string ShowPointsToOwners(Player[] players)
    {
        string message="";
        int[] overallbonus = new int[players.Length];
        for(int i=0; i<buildings.Length; i++)
        {
            if (buildings[i].GetHasOwner())
            {
                
                    
                        overallbonus[buildings[i].GetOwnerIndex()] += buildings[i].pointsforOwner;
                        
                    
               
            }
        }
        for(int i=0; i < overallbonus.Length; i++)
        {
            message += "\nPlayer " + (i+1) + " gains "+overallbonus[i]+" from buildings";
        }
        return message;
    }

    public float FilledBoardPercent()
    {
        float percent=0;
        float all = 0;
        for(int i=0; i<buildings.Length; i++)
        {
            percent += buildings[i].ReturnOccupancy();
            all += buildings[i].capacity;
        }
        return percent/all;
    }
    public void ResetBuildings()
    {
        for(int i=0; i<buildings.Length; i++)
        {
            buildings[i].ResetBuilding();
        }
    }
    public void DisableBuildingButtons(bool setting)
    {
        for(int i=0; i < buildings.Length; i++)
        {
            buildings[i].DisableButtons(setting);
        }
    }
    

   
    


}
