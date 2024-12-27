using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityToolkit.Runtime;

public class Sheep : MonoBehaviour
{
    public Observable<Percent> Hunger = Percent.Max;

    [SerializeField] private Gradient _gradient;
    [SerializeField] private Slider _hungerSlider;
    [SerializeField] private Image _hungerFill;

    private Walk _walk;

    public bool IsHungry => Hunger.Value < 99f;
    public bool IsRunningTask => _currentTask.IsSome(out _);

    private Option<AgentTask> _currentTask = Option<AgentTask>.None;

    #region SheepUI

    private void OnEnable()
    {
        Hunger.OnValueChanged += UpdateHungerUI;
    }

    private void OnDisable()
    {
        Hunger.OnValueChanged -= UpdateHungerUI;
    }

    private void UpdateHungerUI(Percent newValue)
    {
        _hungerSlider.value = newValue;
        _hungerFill.color = _gradient.Evaluate(newValue);
    }

    #endregion

    private void Awake()
    {
        _walk = GetComponent<Walk>();
    }

    private void Update()
    {
        Hunger -= Time.deltaTime;
        
        if (Hunger.Value == Percent.Zero)
        {
            Destroy(gameObject);
        }

        if (!_currentTask.IsSome(out AgentTask task))
        {
            return;
        }

        if (task.IsCompleted)
        {
            _currentTask = Option<AgentTask>.None;
            return;
        }

        task.Update();
    }

    public void StartWanderingTask()
    {
        if (IsRunningTask)
        {
            throw new Exception($"{gameObject.name} is already on a task!");
        }

        Vector3 targetPosition = Utility.RandomOnCircle(5f) + transform.position;
        // _walk.Target = Option<Vector3>.Some(targetPosition);
        _currentTask = Option<AgentTask>.Some(new WanderTask(transform, targetPosition));
    }

    public void StartWalkToGrassTask()
    {
        if (IsRunningTask)
        {
            throw new Exception($"{gameObject.name} is already on a task!");
        }

        var allGrass = GameObject.FindGameObjectsWithTag("Grass");

        if (allGrass.Length == 0)
        {
            Debug.Log("No grass found!");
            return;
        }

        Vector3 nearestGrassPosition = allGrass
            .Select(grass => grass.transform.position)
            .OrderBy(grass => Vector3.SqrMagnitude(transform.position - grass))
            .First();
        
        _currentTask = Option<AgentTask>.Some(new WanderTask(transform, nearestGrassPosition));
    }

    public void EatGrass()
    {
        var allGrass = GameObject.FindGameObjectsWithTag("Grass");

        if (allGrass.Length == 0)
        {
            Debug.Log("No grass found!");
            return;
        }

        GameObject grass = allGrass
            .Select(grass => (grass, grass.transform.position))
            .OrderBy(pair => Vector3.SqrMagnitude(transform.position - pair.position))
            .First().grass;
        
        Destroy(grass);
        
        Hunger += 50f;
    }
}