using System;
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

    private void Update()
    {
        Hunger -= Time.deltaTime;
    }
}
