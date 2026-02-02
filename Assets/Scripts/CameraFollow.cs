using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo; 
    private SkeletonRacer _racerInfo; 
    private Camera _cam;
    public float smoothSpeed = 0.125f; 
    private Vector3 _cinematicOffset;
    public Vector3 offset;
    [Header("Modo Cinemático (Replay)")]
    public float cinematicFOV = 25f;
    [Header("Configuração Geral")]
    public float suavidade = 5f;
    public float suavidadeZoom = 2f;
    [Header("1. Fase de Largada (Start)")]
    [Tooltip("Câmera baixa e diagonal traseira")]
    public Vector3 offsetLargada = new Vector3(2.0f, 1.2f, -2.5f);
    public float fovLargada = 60f;
    [Tooltip("Até quantos % da prova dura a transição de saída (ex: 0.15 = 15%)")]
    public float transicaoSaidaPct = 0.15f; 
    [Header("2. Fase de Corrida (Running)")]
    [Tooltip("Visão lateral/diagonal padrão")]
    public Vector3 offsetCorrida = new Vector3(9f, 4f, -3f);
    public float fovCorrida = 60f;

    [Header("3. Fase de Chegada (Finish)")]
    [Tooltip("Zoom in e mais próximo")]
    public Vector3 offsetChegada = new Vector3(5f, 2.5f, 1.5f);
    public float fovChegada = 50f; 
    [Tooltip("A partir de quantos % da prova começa o zoom final (ex: 0.85 = 85%)")]
    public float inicioZoomFinalPct = 0.85f;

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (alvo != null)
        {
            _racerInfo = alvo.GetComponent<SkeletonRacer>();
            if (_racerInfo == null) _racerInfo = alvo.GetComponentInChildren<SkeletonRacer>();
        }
    }

    public void DefinirOffsetCinematico(Vector3 novoOffset)
    {
        _cinematicOffset = novoOffset;
    }

    void LateUpdate()
    {
        if (alvo == null) return;
        if (GameSettings.isCinematicMode)
        {
            GameObject camSpot = GameObject.Find("CamPos_Replay");
            if (camSpot != null)
            {
                Vector3 desiredPos = camSpot.transform.position + _cinematicOffset;
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 3f);
                transform.LookAt(alvo);
                if (_cam != null)
                {
                    _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, cinematicFOV, Time.deltaTime * 2f);
                }
                return; 
            }
            else {
                Debug.LogWarning("--- CAMERA FOLLOW: Modo Cinemático ativo, mas não encontrou 'CamPos_Replay' na cena! ---");
            }
        }
        Vector3 targetOffset = offsetCorrida;
        float targetFOV = fovCorrida;
        if (_racerInfo != null)
        {
            float progresso = 0f;
            if (_racerInfo.distanciaTotalDaProva > 0)
                progresso = Mathf.Clamp01(_racerInfo.distanciaPercorridaTotal / _racerInfo.distanciaTotalDaProva);

            if (_racerInfo.raceState == SkeletonRacer.RaceState.Ready || 
                _racerInfo.raceState == SkeletonRacer.RaceState.Set || 
                _racerInfo.raceState == SkeletonRacer.RaceState.GunFired)
            {
                targetOffset = offsetLargada;
                targetFOV = fovLargada;
            }
            else if (progresso < transicaoSaidaPct)
            {
                float t = progresso / transicaoSaidaPct; 
                t = Mathf.SmoothStep(0, 1, t); 
                targetOffset = Vector3.Lerp(offsetLargada, offsetCorrida, t);
                targetFOV = Mathf.Lerp(fovLargada, fovCorrida, t);
            }
            else if (progresso > inicioZoomFinalPct)
            {
                float t = (progresso - inicioZoomFinalPct) / (1.0f - inicioZoomFinalPct);
                t = Mathf.SmoothStep(0, 1, t);
                targetOffset = Vector3.Lerp(offsetCorrida, offsetChegada, t);
                targetFOV = Mathf.Lerp(fovCorrida, fovChegada, t);
            }
            else
            {
                targetOffset = offsetCorrida;
                targetFOV = fovCorrida;
            }
        }

        // --- APLICAÇÃO ---
        Vector3 desejada = alvo.position + (alvo.rotation * targetOffset);
        
        if (_racerInfo != null && _racerInfo.velocidadeAtual < 0.1f && _racerInfo.distanciaPercorridaTotal < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, desejada, Time.deltaTime * 10f); 
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desejada, Time.deltaTime * suavidade);
        }
        // Rotação 
        Vector3 focoVisual = alvo.position + (Vector3.up * 1.2f);
        Quaternion rotacaoDesejada = Quaternion.LookRotation(focoVisual - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoDesejada, Time.deltaTime * suavidade * 1.5f);
        if (_cam != null)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * suavidadeZoom);
        }
    }
}