using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SineUIControllerTopDownEffects : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public PrefabSpawner prefabSpawnerObject;
    public Text nameInUI;

    private string nameOfThePrafab;

    private void Start()
    {
        //
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        
        // Toggle UI visibility with H key
        if (keyboard != null && keyboard.hKey.wasPressedThisFrame)
        {
            canvasGroup.alpha = 1f - canvasGroup.alpha;
        }
        
        // Navigate effects with D/Right Arrow
        if (keyboard != null && (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame))
        {
            ChangeEffect(true);
        }
        
        // Navigate effects with A/Left Arrow
        if (keyboard != null && (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame))
        {
            ChangeEffect(false);
        }
        
        // Spawn prefab with right mouse button
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
        {
            prefabSpawnerObject.SpawnPrefab();
        }

        nameOfThePrafab = prefabSpawnerObject.nameOfThePrefab;
        nameInUI.text = "Spawn - " + nameOfThePrafab;
    }

    // Change active VFX
    public void ChangeEffect(bool bo)
    {
        prefabSpawnerObject.ChangePrefabIntex(bo);
        nameOfThePrafab = prefabSpawnerObject.nameOfThePrefab;
    }
}
