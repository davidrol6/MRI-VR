using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIObjectiveList : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public Transform contentParent; // Contenedor (ej. dentro de un ScrollView)
    public GameObject objectivePrefab; // Prefab con texto + tick

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Borra lo viejo
        foreach (GameObject go in spawnedItems) Destroy(go);
        spawnedItems.Clear();

        // Crea solo los del rol actual
        foreach (Objective obj in objectiveManager.objectives)
        {
            if (obj.role == objectiveManager.roleProvider.role)
            {
                GameObject go = Instantiate(objectivePrefab, contentParent);
                TMP_Text text = go.GetComponentInChildren<TMP_Text>();
                Image tick = go.transform.Find("TickIcon").GetComponent<Image>();

                text.text = obj.description;
                tick.enabled = obj.isCompleted;

                spawnedItems.Add(go);
            }
        }
    }
}
