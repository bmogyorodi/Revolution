using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Building //Class to represent Buildings in the game and all their relevent data
{
    public string buildingName; //name of the building
    public int capacity; //capacity of the building, number of spaces that can be occupied by players
    public int pointsforOwner; //points awarded at the end of the game for the dominant owner of spaces of the building
    public Button[] occupancy; //collection of buttons representing the spaces, they change color based on their owner
    private int indexofOccupancy=0; //Index of the next button to be occupied (colored), can also be used as number of occupied blocks
    private int ownerIndex=-1; //Index of the player who owns the building (set at the end game), shows -1 if the building has no owner( players are tied for most space)
    

    

    public void PlayerTakeSpace(Player player)
        /*Player occupies a space in the builing, 
        when they get the character being tied to the building,
        only does something when the building still has unoccupied spaces */
    {
        if (!IsBuildingFull())
        {
            Color color = player.playerColor;
            occupancy[indexofOccupancy].image.color = color;
            indexofOccupancy++;
        }
    }
    public int ReturnOccupancy() //return next index of button to be colored, or number of occupied places
    {
        return indexofOccupancy;
    }
    public bool IsBuildingFull() //boolean, shows if the building  is occupied completely or not (all buttons colored by players, or not)
    {
        return capacity == indexofOccupancy;
    }
    public void SelectBuildingSlot(int blockNumber,Button button,Text buildingof) 
        /*Make a selection of a building and color to be selected for  a spy color change 
         The parameters of the selection are just saved in the text below the button, and the color of the button,
         based on this the strategical switch will be made as the player intended.*/
    {
        if (ReturnOccupancy() > blockNumber && button.interactable==true)
        {
            button.image.color = occupancy[blockNumber].image.color;
            buildingof.text =buildingName;
            Debug.Log("Block Occupied");

        }
        Debug.Log("Block Unoccupied");
    }
    public void SelectBuildingSlot(int blockNumber,Button[] buttons,Text [] buildingof,DataController data)
        /*Same method as before only this time taking into account which slot is selected out of the two slots used for swaping two spaces' color*/
    {
        if(ReturnOccupancy()>blockNumber && buttons[0].interactable == true)
        {
            buttons[data.GetSwitch()].image.color = occupancy[blockNumber].image.color;
            buildingof[data.GetSwitch()].text = buildingName;

            Debug.Log("Block Occupied");

        }
        Debug.Log("Block Unoccupied");
    }

    private void  SetOwner(int player) //basic setter of ownerIndex (used as part of another method, kept private)
    {
        ownerIndex = player;
        
    }
    public int GetOwnerIndex() //basic getter of ownerIndex
    {
        return ownerIndex;
    }
    public bool GetHasOwner() // boolean which shows if building has an owner (based on ownerindex)
    {
        return ownerIndex!=-1;
    }
    public void FindOwner(Player[] player) 
        /*
         * Sets the owner of the building based on which player has
         * it's color on the most spaces (returns no owner if players are tied
         */
    {
        int ownerIndex=ReturnOwnerIndex(PlayersBlocks(player));
        if (ownerIndex != -1)
        {
            SetOwner(ownerIndex);
        }

    }
    private int[] PlayersBlocks(Player[] player) // Returns int array with the number of colored blocks each player has
    {
        int[] blocks = new int[player.Length];
        for (int i = 0; i < occupancy.Length; i++)
        {
            for (int j = 0; j < player.Length; j++)
            {
                if (player[j].playerColor == occupancy[i].image.color)
                {
                    blocks[j]++;
                }
            }
        }
        return blocks;
    }
    public float Ownership(Player[] player, int i)
    {
        int[] blocks = PlayersBlocks(player);
        return (float)blocks[i] / capacity;
    }
    
    public float DominantOpponentPower(Player[] player,int i)
    {
        int[] blocks = PlayersBlocks(player);
        int maxvalue = 0;
        for(int j=0; j < player.Length; j++)
        {
            if (j != i)
            {
                if (blocks[j] > maxvalue)
                {
                    maxvalue = blocks[j];
                }
            }
        }
        return (float)maxvalue / capacity;
    }
    public int DominantOpponentIndex(Player[] player,int i)
    {
        int[] blocks = PlayersBlocks(player);
        int maxvalue = 0;
        int maxIndex = 0;
        for (int j = 0; j < player.Length; j++)
        {
            if (j != i)
            {
                if (blocks[j] > maxvalue)
                {
                    maxvalue = blocks[j];
                    maxIndex = j;
                }
                else
                {
                    if (blocks[j] == maxvalue)
                    {
                        if (player[j].GetPoints() > player[maxIndex].GetPoints())
                        {
                            maxIndex = j;
                        }
                    }
                }
            }
        }
        return maxIndex;
    }
    private int ReturnOwnerIndex(int[]blocks) //helper function to return maxIndex, or -1 if two numbers are tied for the first place
    {
        int max=0;
        int maxIndex=-1;
        for (int i=0; i<blocks.Length;i++)
        {
            if (blocks[i] == max)
            {
                maxIndex = -1;
            }
            if (blocks[i] > max)
            {
                max = blocks[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }
    public void ResetBuilding()
    {
        ownerIndex = -1;
        indexofOccupancy = 0;
        for(int i=0; i<capacity; i++)
        {
            occupancy[i].image.color = new Color(255, 255, 255);
        }
    }
    public void DisableButtons(bool setting)
    {
        for(int i=0; i < occupancy.Length; i++)
        {
            occupancy[i].interactable = setting;
        }
    }
   


}
