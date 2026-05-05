using UnityEngine;
using UnityEngine.UIElements;  
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("Configuração da IA")]
    public Vector2 ritmoFacil = new Vector2(0.79f, 0.87f);
    public Vector2 reacaoFacil = new Vector2(0.25f, 0.40f);
    public Vector2 ritmoMedio = new Vector2(0.87f, 0.92f);
    public Vector2 reacaoMedio = new Vector2(0.16f, 0.24f);
    public Vector2 ritmoDificil = new Vector2(0.915f, 0.965f);  
    public Vector2 reacaoDificil = new Vector2(0.145f, 0.175f);

    [Header("UI Toolkit References")]
    public UIDocument resultadosDocument; 

    private List<SkeletonRacer> competidores = new List<SkeletonRacer>();
    private List<SkeletonRacer> classificados = new List<SkeletonRacer>();
    private Transform _cameraDummyTarget; 
    private SkeletonRacer _focoAtualRacer;
    private float delayToSet = 3.0f; 
    private float delayToGun = 0f;   

    void Start()
    {
        if (resultadosDocument != null) 
            resultadosDocument.rootVisualElement.style.display = DisplayStyle.None;
        GameObject dummy = new GameObject("Camera_Virtual_Target");
        _cameraDummyTarget = dummy.transform;
        SkeletonRacer player = FindObjectOfType<SkeletonRacer>();
        if (player == null) return;
        if (GameSettings.isCinematicMode)
        {
            ConfigurarCenaFilme(player);
        }
        else if (GameSettings.provaSelecionada == SkeletonRacer.TipoDeProva.Revezamento_4x100m)
        {
            ConfigurarRevezamento(player);
        }
        else
        {
            // Modo Sprint Normal
            competidores.Add(player);
            GerarOponentes(player);
            _focoAtualRacer = player;
            _cameraDummyTarget.position = player.transform.position;
            _cameraDummyTarget.rotation = player.transform.rotation;

            StartCoroutine(SequenciaLargada());
        }
    }
    
    void Update()
    {
        if (_cameraDummyTarget != null && _focoAtualRacer != null)
        {
            _cameraDummyTarget.position = Vector3.Lerp(_cameraDummyTarget.position, _focoAtualRacer.transform.position, Time.deltaTime * 5f);
            _cameraDummyTarget.rotation = _focoAtualRacer.transform.rotation;
        }
    }

    void ConfigurarCenaFilme(SkeletonRacer playerOriginal)
    {
        competidores.Clear();
        classificados.Clear();
        
        foreach (var time in GameSettings.timesReplay)
        {
            SkeletonRacer corredorAnterior = null;

            for (int perna = 1; perna <= 4; perna++)
            {
                SkeletonRacer novoCorredor;
                bool ehTimeDoJogador = (time.raia == GameSettings.raiaDoJogador); 
                if (ehTimeDoJogador)
                {
                    if (perna == 1) novoCorredor = playerOriginal;
                    else 
                    {
                        GameObject obj = Instantiate(playerOriginal.gameObject);
                        var cf = obj.GetComponent<CameraFollow>(); if (cf) Destroy(cf);
                        var al = obj.GetComponent<AudioListener>(); if (al) Destroy(al);
                        novoCorredor = obj.GetComponent<SkeletonRacer>();
                    }
                    novoCorredor.isBot = false; 
                }
                else
                {
                    // Inimigos
                    GameObject obj = Instantiate(playerOriginal.gameObject);
                    var cf = obj.GetComponent<CameraFollow>(); if (cf) Destroy(cf);
                    var al = obj.GetComponent<AudioListener>(); if (al) Destroy(al);
                    novoCorredor = obj.GetComponent<SkeletonRacer>();
                    novoCorredor.isBot = true; 
                }
                // Configuração Visual e Técnica
                novoCorredor.raiaAlvo = time.raia;
                novoCorredor.provaSelecionada = SkeletonRacer.TipoDeProva.Revezamento_4x100m;
                novoCorredor.numeroPerna = perna;
                if (perna == 4)
                    novoCorredor.nomeCorredor = $"{time.nomeEquipe}";
                else
                    novoCorredor.nomeCorredor = $"{time.nomeEquipe} (P{perna})";
                novoCorredor.corForcada = time.corTronco;
                novoCorredor.tempoAlvo = time.tempos[perna - 1]; 
                novoCorredor.ConfigurarProva();
                if (perna == 1)
                {
                    novoCorredor.distanciaPercorridaTotal = 0; 
                    float start400 = novoCorredor.trackManager.GetPontoDeLargada(400f, time.raia);
                    SetOffsetLargadaManual(novoCorredor, start400);
                    novoCorredor.raceState = SkeletonRacer.RaceState.Ready;
                    competidores.Add(novoCorredor); 
                    if (ehTimeDoJogador) 
                    {
                        var builder = novoCorredor.GetComponent<SkeletonBuilder>();
                        if (builder) {
                            builder.corPlayer = time.corTronco;
                            builder.BuildSkeleton(); 
                        }
                        _focoAtualRacer = novoCorredor;
                        _cameraDummyTarget.position = novoCorredor.transform.position;
                        _cameraDummyTarget.rotation = novoCorredor.transform.rotation;
                        StartCoroutine(AplicarCameraCinematica(novoCorredor));
                    }
                }
                else if (perna == 2) SetupPernaRevezamento(novoCorredor, corredorAnterior, 90f, 80f, time.raia);
                else if (perna == 3) SetupPernaRevezamento(novoCorredor, corredorAnterior, 190f, 180f, time.raia);
                else if (perna == 4) SetupPernaRevezamento(novoCorredor, corredorAnterior, 290f, 280f, time.raia);

                corredorAnterior = novoCorredor;
            }
        }
        StartCoroutine(SequenciaLargada());
    }

    System.Collections.IEnumerator AplicarCameraCinematica(SkeletonRacer alvoInicial)
    {
        yield return null; 
        yield return null;
        GameObject camSpot = GameObject.Find("CamPos_Replay");
        var camScript = Camera.main.GetComponent<CameraFollow>();

        if (camSpot != null && camScript != null && _cameraDummyTarget != null)
        {
            camScript.alvo = _cameraDummyTarget;
            Vector3 offsetCalculado = camSpot.transform.position - alvoInicial.transform.position;
            camScript.DefinirOffsetCinematico(offsetCalculado);
            camScript.transform.position = camSpot.transform.position;
            camScript.transform.LookAt(_cameraDummyTarget); 
        }
        else
        {
            Debug.LogError("❌ ERRO CÂMERA: Verifique se existe o objeto 'CamPos_Replay' e se a MainCamera tem o script 'CameraFollow'.");
        }
    }

    System.Collections.IEnumerator AjustarCameraDelay(SkeletonRacer alvo)
    {
        yield return null; 
        AjustarCameraPeloObjeto(alvo);
    }

    void AjustarCameraPeloObjeto(SkeletonRacer alvo)
    {
        GameObject camSpot = GameObject.Find("CamPos_Replay");
        var camScript = Camera.main.GetComponent<CameraFollow>();

        if (camSpot != null && camScript != null)
        {
            camScript.offset = camSpot.transform.position - alvo.transform.position;
            camScript.transform.position = camSpot.transform.position; 
            camScript.smoothSpeed = 0.05f; 
        }
        else
        {
            Debug.LogWarning("⚠️ Objeto 'CamPos_Replay' não encontrado! Usando câmera padrão.");
            if(camScript) camScript.offset = new Vector3(15f, 12f, -25f);
        }
    }

    System.Collections.IEnumerator SequenciaLargada()
    {
        Debug.Log("JUIZ: On Your Marks...");
        if (AudioManager.instance) AudioManager.instance.TocarSFX(AudioManager.instance.voiceOnYourMarks);
        yield return new WaitForSeconds(1.0f); 
        if (AudioManager.instance) AudioManager.instance.SilenciarMusica();
        yield return new WaitForSeconds(delayToSet);
        Debug.Log("JUIZ: Set...");
        if (AudioManager.instance) AudioManager.instance.TocarSFX(AudioManager.instance.voiceSet);
        foreach (var c in competidores) c.OuvirComandoSet();
        delayToGun = Random.Range(1.5f, 2.5f);
        yield return new WaitForSeconds(delayToGun);
        Debug.Log("JUIZ: BANG! 🔫");
        if (AudioManager.instance) 
        {
            AudioManager.instance.TocarSFX(AudioManager.instance.gunShot); 
            if (GameSettings.provaSelecionada == SkeletonRacer.TipoDeProva.Revezamento_4x100m)
            {
                AudioManager.instance.TocarAmbienteCorrida(SkeletonRacer.TipoDeProva.SprintLongo_400m);
            }
            else
            {
                AudioManager.instance.TocarAmbienteCorrida(GameSettings.provaSelecionada);
            }
        }
        foreach (var c in competidores) c.OuvirTiroDeLargada();
    }

    void GerarOponentes(SkeletonRacer player)
    {
        int totalRaias = player.trackManager ? player.trackManager.numeroRaias : 8;
        Vector2 faixaRitmoAtual;
        Vector2 faixaReacaoAtual;

        switch (GameSettings.dificuldadeSelecionada)
        {
            case GameSettings.Difficulty.Facil:
                faixaRitmoAtual = ritmoFacil;
                faixaReacaoAtual = reacaoFacil;
                break;
            case GameSettings.Difficulty.Dificil:
                faixaRitmoAtual = ritmoDificil;
                faixaReacaoAtual = reacaoDificil;
                break;
            case GameSettings.Difficulty.Medio:
            default:
                faixaRitmoAtual = ritmoMedio;
                faixaReacaoAtual = reacaoMedio;
                break;
        }

        for (int i = 1; i <= totalRaias; i++) {
            if (i == player.raiaAlvo) continue; 
            GameObject botObj = Instantiate(player.gameObject);
            botObj.name = $"CPU_Raia_{i}";
            var cf = botObj.GetComponent<CameraFollow>(); if(cf) Destroy(cf);
            var al = botObj.GetComponent<AudioListener>(); if(al) Destroy(al);

            SkeletonRacer botRacer = botObj.GetComponent<SkeletonRacer>();
            botRacer.isBot = true;
            botRacer.nomeCorredor = $"Atleta {i}"; 
            botRacer.raiaAlvo = i;
            botRacer.modoAutomatico = true;
            botRacer.usarAutoClick = false;
            botRacer.ritmoAutomatico = Random.Range(faixaRitmoAtual.x, faixaRitmoAtual.y);
            botRacer.tempoReacaoAuto = Random.Range(faixaReacaoAtual.x, faixaReacaoAtual.y);
            botRacer.provaSelecionada = player.provaSelecionada;
            botRacer.ConfigurarProva(); 
            competidores.Add(botRacer);
        }
    }

    void ConfigurarRevezamento(SkeletonRacer playerOriginal)
    {
        competidores.Clear();
        classificados.Clear();
        int totalRaias = playerOriginal.trackManager ? playerOriginal.trackManager.numeroRaias : 8;
        Vector2 fRitmo = ritmoMedio; Vector2 fReacao = reacaoMedio;
        if (GameSettings.dificuldadeSelecionada == GameSettings.Difficulty.Facil) { fRitmo = ritmoFacil; fReacao = reacaoFacil; }
        if (GameSettings.dificuldadeSelecionada == GameSettings.Difficulty.Dificil) { fRitmo = ritmoDificil; fReacao = reacaoDificil; }
        for (int raia = 1; raia <= totalRaias; raia++)
        {
            bool ehTimeDoPlayer = (raia == playerOriginal.raiaAlvo);
            SkeletonRacer corredorAnterior = null;
            for (int perna = 1; perna <= 4; perna++)
            {
                SkeletonRacer novoCorredor;
                if (ehTimeDoPlayer && perna == 1)
                {
                    novoCorredor = playerOriginal;
                    _focoAtualRacer = novoCorredor;
                }
                else
                {
                    GameObject obj = Instantiate(playerOriginal.gameObject);
                    var cf = obj.GetComponent<CameraFollow>(); if (cf) Destroy(cf);
                    var al = obj.GetComponent<AudioListener>(); if (al) Destroy(al);
                    novoCorredor = obj.GetComponent<SkeletonRacer>();
                }
                // Configurações Básicas
                novoCorredor.raiaAlvo = raia;
                novoCorredor.provaSelecionada = SkeletonRacer.TipoDeProva.Revezamento_4x100m;
                novoCorredor.numeroPerna = perna;
                novoCorredor.ConfigurarProva();
                if (ehTimeDoPlayer)
                {
                    novoCorredor.isBot = false; 
                    novoCorredor.nomeCorredor = (perna == 4) ? "Player (Final)" : $"P{perna} (Player)";
                    novoCorredor.clicksPorSegundo = GameSettings.clicksPorSegundo;
                    novoCorredor.modoAutomatico = GameSettings.modoAutomatico;
                    novoCorredor.usarAutoClick = GameSettings.usarAutoClick;
                }
                else
                {
                    novoCorredor.isBot = true;
                    novoCorredor.nomeCorredor = (perna == 4) ? $"CPU {raia} (Final)" : $"CPU {raia}-{perna}";
                    novoCorredor.modoAutomatico = true;
                    novoCorredor.usarAutoClick = false;
                    novoCorredor.ritmoAutomatico = Random.Range(fRitmo.x, fRitmo.y);
                    novoCorredor.tempoReacaoAuto = Random.Range(fReacao.x, fReacao.y);
                }
                // --- POSICIONAMENTO E LOGICA DE BASTÃO ---
                if (perna == 1)
                {
                    novoCorredor.distanciaPercorridaTotal = 0; 
                    float start400 = novoCorredor.trackManager.GetPontoDeLargada(400f, raia);
                    SetOffsetLargadaManual(novoCorredor, start400);
                    novoCorredor.raceState = SkeletonRacer.RaceState.Ready; 
                    competidores.Add(novoCorredor); 
                }
                else if (perna == 2) SetupPernaRevezamento(novoCorredor, corredorAnterior, 90f, 80f, raia);
                else if (perna == 3) SetupPernaRevezamento(novoCorredor, corredorAnterior, 190f, 180f, raia);
                else if (perna == 4) SetupPernaRevezamento(novoCorredor, corredorAnterior, 290f, 280f, raia);
                corredorAnterior = novoCorredor;
            }
        }
        StartCoroutine(SequenciaLargada());
    }

    void SetupPernaRevezamento(SkeletonRacer r, SkeletonRacer anterior, float posInicial, float gatilho, int raia)
    {
        r.raceState = SkeletonRacer.RaceState.WaitingHandoff; 
        r.colegaAnterior = anterior;
        r.distanciaGatilho = gatilho; 
        r.ConfigurarProva();
        r.distanciaPercorridaTotal = posInicial; 
    }

    void SetOffsetLargadaManual(SkeletonRacer r, float offset)
    {
        r.ConfigurarProva();
    }

    public void TrocarFocoCamera(SkeletonRacer novoFoco)
    {
        _focoAtualRacer = novoFoco;
        CameraFollow cam = null;
        if (Camera.main != null) cam = Camera.main.GetComponent<CameraFollow>();
        if (cam == null) cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            cam.alvo = _cameraDummyTarget; 
            HUDController hud = FindObjectOfType<HUDController>();
            if (hud != null) hud.AtualizarFocoJogador(novoFoco);
        }
    }

    private bool _exibirResultados = false;

    public void RegistrarQueimaLargada(SkeletonRacer corredor)
    {
        if (!corredor.isBot)
        {
            _exibirResultados = true;
            if (!classificados.Contains(corredor)) classificados.Add(corredor); 
            AtualizarTelaResultados();
        }
    }

    public void RegistrarChegada(SkeletonRacer corredor)
    {
        if (!classificados.Contains(corredor))
        {
            classificados.Add(corredor);

            if (!corredor.isBot) 
            {
                _exibirResultados = true;
            }

            if (_exibirResultados)
            {
                StartCoroutine(RotinaExibirResultados());
            }
        }
    }

    System.Collections.IEnumerator RotinaExibirResultados()
    {
        yield return new WaitForSeconds(0.5f);
        AtualizarTelaResultados();
    }

    void AtualizarTelaResultados()
    {
        if (resultadosDocument == null) return;
        if (resultadosDocument.rootVisualElement.style.display == DisplayStyle.None)
        {
             if (AudioManager.instance) AudioManager.instance.TocarFinal();
        }
        // 1. Ativa o Painel 
        var root = resultadosDocument.rootVisualElement;
        root.style.display = DisplayStyle.Flex;
        var listContainer = root.Q<ScrollView>("ResultsList");
        var btnBack = root.Q<Button>("BtnBack");

        // Configura botão de voltar 
        btnBack.clicked -= VoltarMenu; 
        btnBack.clicked += VoltarMenu;
        listContainer.Clear();
        var listaFiltrada = classificados;
        
        if (GameSettings.provaSelecionada == SkeletonRacer.TipoDeProva.Revezamento_4x100m)
        {
            listaFiltrada = classificados.Where(r => r.numeroPerna == 4).ToList();
        }

        var listaOrdenada = listaFiltrada
        .OrderBy(x => x.raceState == SkeletonRacer.RaceState.FalseStart) 
        .ThenBy(x => x.tempoCorrida)
        .ToList();

        for (int i = 0; i < listaOrdenada.Count; i++)
        {
            SkeletonRacer r = listaOrdenada[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("result-row");
            VisualElement badge = new VisualElement();
            badge.AddToClassList("rank-badge");
            Label rankLbl = new Label((i + 1).ToString());
            rankLbl.AddToClassList("rank-text");
            badge.Add(rankLbl);
            row.Add(badge);
            Label nameLbl = new Label(r.nomeCorredor);
            nameLbl.AddToClassList("row-text");
            nameLbl.AddToClassList("name-col");
            row.Add(nameLbl);
            VisualElement sep1 = new VisualElement(); sep1.AddToClassList("separator"); row.Add(sep1);

            // TEMPO
            string textoTempo;
            if (r.raceState == SkeletonRacer.RaceState.FalseStart) 
            {
                textoTempo = "DQ (Queima)";
            }
            else 
            {
                textoTempo = $"{r.tempoCorrida:F2} s";
            }

            Label timeLbl = new Label(textoTempo);
            timeLbl.AddToClassList("row-text");
            timeLbl.AddToClassList("time-col");
            row.Add(timeLbl);

            // SEPARADOR
            VisualElement sep2 = new VisualElement(); sep2.AddToClassList("separator"); row.Add(sep2);

            // RAIA
            Label laneLbl = new Label($"Raia {r.raiaAlvo}");
            laneLbl.AddToClassList("lane-col");
            row.Add(laneLbl);

            // Adiciona a linha na lista principal
            listContainer.Add(row);
        }
    }

    void VoltarMenu()
    {
        if (AudioManager.instance) AudioManager.instance.TocarMenu();
        SceneManager.LoadScene("Menu");
    }
}