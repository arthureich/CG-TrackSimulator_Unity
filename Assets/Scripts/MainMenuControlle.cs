using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; 

public class MainMenuController : MonoBehaviour
{
    private UIDocument _doc;
    private DropdownField _dropdownProva;
    private DropdownField _dropdownRaia;
    private DropdownField _dropdownDificuldade;
    private Toggle _toggleAutoClick;
    private Toggle _toggleAutomatico;
    private Button _btnStart;
    private Button _btnReplay;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        
        if (_doc == null)
        {
            return;
        }

        var root = _doc.rootVisualElement;
        _dropdownDificuldade = root.Q<DropdownField>("DropdownDificuldade");
        _dropdownProva = root.Q<DropdownField>("DropdownProva");
        _dropdownRaia = root.Q<DropdownField>("DropdownRaia");
        _toggleAutoClick = root.Q<Toggle>("ToggleAutoClick");
        _toggleAutomatico = root.Q<Toggle>("ToggleAutomatico");
        _btnStart = root.Q<Button>("BtnStart");
        if (_btnStart != null) { _btnStart.clicked += IniciarCorrida; }
        _btnReplay = root.Q<Button>("BtnReplay"); 
        if (_btnReplay != null) _btnReplay.clicked += IniciarModoCinematico;
        if (_btnStart != null)
        {
            _btnStart.clicked += IniciarCorrida;
        }
    }

    void IniciarModoCinematico()
    {
        GameSettings.isCinematicMode = true; 
        GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.Revezamento_4x100m;
        SceneManager.LoadScene("Game");
    }

    void IniciarCorrida()
    {
        string provaTexto = _dropdownProva.value;
        switch (provaTexto)
        {
            case "100m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.Sprint_100m; break;
            case "200m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.Sprint_200m; break;
            case "400m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.SprintLongo_400m; break;
            case "800m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.MeioFundo_800m; break;
            case "1500m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.MeioFundo_1500m; break;
            case "4x100m": GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.Revezamento_4x100m; break;
            default: GameSettings.provaSelecionada = SkeletonRacer.TipoDeProva.Sprint_100m; break;
        }

        GameSettings.raiaDoJogador = _dropdownRaia.index + 1;
        GameSettings.usarAutoClick = _toggleAutoClick.value;
        GameSettings.modoAutomatico = _toggleAutomatico.value;

        string difTexto = _dropdownDificuldade.value;
        switch (difTexto)
        {
            case "Fácil": GameSettings.dificuldadeSelecionada = GameSettings.Difficulty.Facil; break;
            case "Médio": GameSettings.dificuldadeSelecionada = GameSettings.Difficulty.Medio; break;
            case "Difícil": GameSettings.dificuldadeSelecionada = GameSettings.Difficulty.Dificil; break;
            default: GameSettings.dificuldadeSelecionada = GameSettings.Difficulty.Medio; break;
        }
        SceneManager.LoadScene("Game");
    }

    void OnDisable()
    {
        if (_btnStart != null)
        {
            _btnStart.clicked -= IniciarCorrida;
        }
    }
}