using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _root;
    
    // Elementos da UI
    private Label _timeLabel;
    private Label _distLabel;
    private Label _speedValue;
    private Label _staminaValue;
    private VisualElement _speedBarFill;
    private VisualElement _staminaBarFill;
    private SkeletonRacer _player;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        if (_doc == null) return;

        _root = _doc.rootVisualElement;

        _timeLabel = _root.Q<Label>("TimeLabel");
        _distLabel = _root.Q<Label>("DistLabel");
        _speedValue = _root.Q<Label>("SpeedValue");
        _staminaValue = _root.Q<Label>("StaminaValue");
        _speedBarFill = _root.Q<VisualElement>("SpeedBarFill");
        _staminaBarFill = _root.Q<VisualElement>("StaminaBarFill");
    }

    void Start()
    {
        // Busca inicial (Perna 1)
        BuscarJogadorInicial();
    }

    void BuscarJogadorInicial()
    {
        var racers = FindObjectsOfType<SkeletonRacer>();
        foreach (var r in racers)
        {
            // Pega o primeiro humano que encontrar
            if (!r.isBot)
            {
                _player = r;
                break;
            }
        }
    }

    // --- NOVO MÉTODO: Chamado pelo RaceManager ---
    public void AtualizarFocoJogador(SkeletonRacer novoPlayer)
    {
        _player = novoPlayer;
        if (_root != null) _root.style.display = DisplayStyle.Flex; // Garante que a UI reapareça
    }
    // ---------------------------------------------

    void Update()
    {
        if (_player == null) return;

        if (_root != null)
        {
            if (_player.raceState == SkeletonRacer.RaceState.Finished || _player.raceState == SkeletonRacer.RaceState.FalseStart)
            {
            }
            else
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        if (_root.style.display == DisplayStyle.None) return;

        // 2. ATUALIZA TEXTOS
        _timeLabel.text = $"{_player.tempoCorrida:F2} s";
        _distLabel.text = $"{_player.distanciaPercorridaTotal:F1} m";
        
        _speedValue.text = $"{_player.velocidadeAtual:F1} m/s";
        _staminaValue.text = $"{_player.stamina:F0}%";
        
        float speedPct = Mathf.Clamp01(_player.velocidadeAtual / _player.velocidadeMaximaCap);
        if(_speedBarFill != null) _speedBarFill.style.width = Length.Percent(speedPct * 100f);
        
        float stamPct = Mathf.Clamp01(_player.stamina / 100f);
        if(_staminaBarFill != null) 
        {
            _staminaBarFill.style.width = Length.Percent(stamPct * 100f);
            Color stamColor = Color.Lerp(Color.red, Color.green, stamPct);
            _staminaBarFill.style.backgroundColor = stamColor;
        }
    }
}