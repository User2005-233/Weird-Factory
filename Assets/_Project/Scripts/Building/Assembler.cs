using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Assembler : Building, IDirectionProvider, IInputPortProvider, IOutputPortProvider
{
    public GridDirection Facing => facing;
    public Recipe currentRecipe;
    float recipeInterval;
    Timer timer;

    public override float ProcessingProgress => timer?.Progress ?? 0f;
    public override bool IsProcessing => timer != null && timer.IsRunning;

    public void SetRecipe(string id){
        currentRecipe = BuildingManager.Instance.registry.GetRecipe(id);
        PlayerInventory.Instance.AddItem(ClearInputBuffer());
        recipeInterval = currentRecipe.processTime;
        foreach(var itemType in currentRecipe.inputTypes){
            inputBuffer[itemType] = 0;
        }
        foreach(var itemType in currentRecipe.outputTypes){
            outputBuffer[itemType] = 0;
        }
        timer = Timer.Register(recipeInterval, Process, false);
    }

    public Dictionary<ItemType, int> ClearInputBuffer()
    {
        if(currentRecipe == null) return null;
        var result = new Dictionary<ItemType, int>();
        foreach(var itemType in currentRecipe.inputTypes)
        {
            result[itemType] = inputBuffer[itemType];
            inputBuffer.Remove(itemType);
        }
        foreach(var itemType in currentRecipe.outputTypes)
        {
            result[itemType] = outputBuffer[itemType];
            outputBuffer.Remove(itemType);
        }
        return result;
    }

    void Process()
    {
        if(currentRecipe == null) return;
        for(int i = 0; i < currentRecipe.inputTypes.Length; i++){
            if(inputBuffer[currentRecipe.inputTypes[i]] < currentRecipe.inputAmounts[i]) return;
        }
        for(int i = 0; i < currentRecipe.inputTypes.Length; i++){
            inputBuffer[currentRecipe.inputTypes[i]] -= currentRecipe.inputAmounts[i];
        }
        for(int i = 0; i < currentRecipe.outputTypes.Length; i++){
            outputBuffer[currentRecipe.outputTypes[i]] += currentRecipe.outputAmounts[i];
        }
    }
    
    public bool HasInputPort(GridDirection worldDir)
    {
        return IsInputSide(worldDir);
    }
    
    public bool HasOutputPort(GridDirection worldDir)
    {
        return IsOutputSide(worldDir);
    }
    
    public bool TryInsert(ItemType itemType)
    {
        if(currentRecipe == null) return false;
        if(!currentRecipe.inputTypes.Contains(itemType)) return false;
        if(inputBuffer.GetValueOrDefault(itemType, 0) >= itemType.maxStackSize) return false;
        inputBuffer[itemType] = inputBuffer.GetValueOrDefault(itemType, 0) + 1;
        return true;
    }

    public bool TryExtract(out ItemType itemType)
    {
        if(currentRecipe == null){
            itemType = null;
            return false;
        }
        if (outputBuffer.GetValueOrDefault(currentRecipe.outputTypes[0], 0) <= 0) 
        { 
            itemType = null; 
            return false; 
        }
        itemType = currentRecipe.outputTypes[0];
        outputBuffer[itemType]--;
        return true;
    }
    
    public override void OnTick(float deltaTime)
    {
        if(timer == null) return;
        if(!timer.IsRunning){
            if(currentRecipe != null){
                if(CheckIngredients()){
                    timer.Start();
                }
            }
        }
        timer.Tick(deltaTime);
    }

    bool CheckIngredients()
    {
        if(currentRecipe == null) return false;
        for(int i = 0; i < currentRecipe.inputTypes.Length; i++){
            if(inputBuffer.GetValueOrDefault(currentRecipe.inputTypes[i], 0) < currentRecipe.inputAmounts[i]) return false;
        }
        return true;
    }
}