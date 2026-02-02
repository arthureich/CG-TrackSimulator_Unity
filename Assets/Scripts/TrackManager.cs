using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteAlways] 
public class TrackManager : MonoBehaviour
{
    [Header("Dimensões Oficiais (IAAF)")]
    public float raioCurva = 36.50f;
    public float comprimentoReta = 84.39f;
    public int numeroRaias = 8;
    public float larguraRaia = 1.22f;
    
    [Header("Extensão (Chute dos 100m)")]
    public float extensaoChute = 30.0f; 

    [Header("Qualidade")]
    public int resolucaoCurva = 60; 

    [Header("Materiais")]
    public Material materialPista; 
    public Material materialLinha; 
    public Material materialChegada; 
    
    public Color corStart100m = Color.white;
    public Color corStart200m = Color.green;
    public Color corStart400m = Color.blue;
    public Color corStart1500m = Color.yellow;

    private float _perimetroRaiaZero;

    void Awake() { ReconstruirPista(); }

    void OnValidate()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) ReconstruirPista(); };
        #endif
    }

    public void ReconstruirPista()
    {
        _perimetroRaiaZero = (2 * comprimentoReta) + (2 * Mathf.PI * raioCurva);
        
        GerarMalhaPista();
        GerarLinhasRaias();
        GerarMarcacoes(); 
    }

    // --- POSIÇÃO DE LARGADA ---
    public float GetPontoDeLargada(float distanciaDaProva, int raia)
    {
        float indiceRaia = (float)raia - 0.5f; 
        float raioR = raioCurva + (indiceRaia * larguraRaia);
        float perimetroR = (2 * comprimentoReta) + (2 * Mathf.PI * raioR);
        float posChegada = comprimentoReta; 

        if (Mathf.Abs(distanciaDaProva - 100f) < 1f)
        {
            return posChegada - distanciaDaProva;
        }

        float pontoLargada = posChegada - distanciaDaProva;
        while (pontoLargada < 0) pontoLargada += perimetroR;
        pontoLargada %= perimetroR;

        return pontoLargada;
    }

    void GerarMarcacoes()
    {
        GameObject container = GameObject.Find("Marcacoes_Pista");
        if (container) DestroyImmediate(container);
        container = new GameObject("Marcacoes_Pista");
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        // 1. CHEGADA (Final da Reta 1)
        CriarLinhaTransversal(container, comprimentoReta, Color.white, "Finish_Line", true);

        // 2. LARGADAS
        for (int r = 1; r <= numeroRaias; r++) 
        {
            // 100m: Agora vai retornar negativo (ex: -15), caindo na extensão reta
            float start100 = GetPontoDeLargada(100f, r);
            CriarMarcaNoChao(container, start100, r - 0.5f, corStart100m, $"Start_100m_R{r}");

            // Outras provas
            CriarMarcaNoChao(container, GetPontoDeLargada(200f, r), r - 0.5f, corStart200m, $"Start_200m_R{r}");
            CriarMarcaNoChao(container, GetPontoDeLargada(400f, r), r - 0.5f, corStart400m, $"Start_400m_R{r}");
            CriarMarcaNoChao(container, GetPontoDeLargada(1500f, r), r - 0.5f, corStart1500m, $"Start_1500m_R{r}");
        }
    }

    private Vector3 CalcularPosicaoGeometrica(float d, float raiaIndexFloat)
    {
        float raioDaRaia = raioCurva + (raiaIndexFloat * larguraRaia);
        float compCurva = Mathf.PI * raioDaRaia;
        float alturaBase = 0.01f; 

        // 1. EXTENSÃO (CHUTE) 
        if (d < 0)
        {
            return new Vector3(d - (comprimentoReta * 0.5f), alturaBase, -raioDaRaia);
        }
        
        // 2. OVAL (LOOP PADRÃO)
        if (d < comprimentoReta) {
            // Reta Principal
            return new Vector3(d - (comprimentoReta * 0.5f), alturaBase, -raioDaRaia);
        }
        else if (d < comprimentoReta + compCurva) {
            // Curva 1
            float progresso = (d - comprimentoReta) / compCurva;
            float angulo = Mathf.Lerp(-Mathf.PI/2, Mathf.PI/2, progresso);
            return new Vector3((comprimentoReta * 0.5f) + Mathf.Cos(angulo) * raioDaRaia, alturaBase, Mathf.Sin(angulo) * raioDaRaia);
        }
        else if (d < (comprimentoReta * 2) + compCurva) {
            // Reta Oposta
            float dReta = d - (comprimentoReta + compCurva);
            return new Vector3((comprimentoReta * 0.5f) - dReta, alturaBase, raioDaRaia);
        }
        else {
            // Curva 2
            float progresso = (d - ((comprimentoReta * 2) + compCurva)) / compCurva;
            float angulo = Mathf.Lerp(Mathf.PI/2, 3*Mathf.PI/2, progresso);
            return new Vector3((-comprimentoReta * 0.5f) + Mathf.Cos(angulo) * raioDaRaia, alturaBase, Mathf.Sin(angulo) * raioDaRaia);
        }
    }

    // --- MALHA  ---
    void GerarMalhaPista()
    {
        Mesh mesh = new Mesh(); mesh.name = "ProceduralTrack";
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // PARTE 1: GERAR O OVAL (0 até Perimetro)
        // Isso garante que a curva feche perfeitamente sem saber que existe extensão.
        int segsOval = (resolucaoCurva * 2) + 8;
        int ovalVertOffset = 0; 

        for (int i = 0; i <= segsOval; i++)
        {
            float t = (float)i / segsOval;
            // Distância exata no loop
            float distBase = t * ((2*comprimentoReta) + (2*Mathf.PI*raioCurva)); 

            // Calcula geometria normal
            PosicaoRotacao pIn = CalcularRotaGeometrica(distBase, 0);
            PosicaoRotacao pOut = CalcularRotaGeometrica(distBase, (float)numeroRaias);

            verts.Add(transform.InverseTransformPoint(pIn.posicao));
            verts.Add(transform.InverseTransformPoint(pOut.posicao));
            uvs.Add(new Vector2(0, distBase * 0.1f));
            uvs.Add(new Vector2(1, distBase * 0.1f));
        }

        // Triângulos do Oval
        for (int i = 0; i < segsOval; i++) {
            AddQuad(tris, i * 2);
        }

        // PARTE 2: GERAR O CHUTE (Separado!)
        // Isso cria vértices novos, desconectados do loop do oval, evitando a distorção.
        int chuteVertStart = verts.Count;
        int segsChute = 2; // Reta simples, não precisa de muitos segmentos

        for (int i = 0; i <= segsChute; i++)
        {
            float t = (float)i / segsChute;
            // Vai de -Extensao até 0
            float d = Mathf.Lerp(-extensaoChute, 0, t);
            
            PosicaoRotacao pIn = CalcularRotaGeometrica(d, 0); 
            PosicaoRotacao pOut = CalcularRotaGeometrica(d, (float)numeroRaias);

            verts.Add(transform.InverseTransformPoint(pIn.posicao));
            verts.Add(transform.InverseTransformPoint(pOut.posicao));
            uvs.Add(new Vector2(0, d * 0.1f)); 
            uvs.Add(new Vector2(1, d * 0.1f));
        }

        // Triângulos do Chute
        for (int i = 0; i < segsChute; i++) {
            AddQuad(tris, chuteVertStart + (i * 2));
        }

        mesh.vertices = verts.ToArray(); mesh.triangles = tris.ToArray(); mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        
        if(GetComponent<MeshFilter>()) GetComponent<MeshFilter>().mesh = mesh;
        var rend = GetComponent<MeshRenderer>();
        if(rend) {
            //rend.material = materialPista;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // Sem sombra própria
            rend.receiveShadows = true;
        }
    }

    void AddQuad(List<int> tris, int root)
    {
        tris.Add(root); tris.Add(root + 1); tris.Add(root + 2);
        tris.Add(root + 1); tris.Add(root + 3); tris.Add(root + 2);
    }

    // --- LINHAS ---
    void GerarLinhasRaias()
    {
        var temp = new List<GameObject>();
        foreach(Transform child in transform) { if (child.name.StartsWith("Linha_") || child.name == "Linhas") temp.Add(child.gameObject); }
        temp.ForEach(child => DestroyImmediate(child));
        GameObject parent = new GameObject("Linhas"); parent.transform.parent = transform; parent.transform.localPosition = Vector3.zero; parent.transform.localRotation = Quaternion.identity;

        for (int r = 0; r <= numeroRaias; r++)
        {
            GameObject obj = new GameObject($"Linha_{r}"); obj.transform.parent = parent.transform; obj.transform.localPosition = Vector3.zero; obj.transform.localRotation = Quaternion.identity;
            LineRenderer lr = obj.AddComponent<LineRenderer>(); lr.useWorldSpace = false; lr.material = materialLinha; lr.startWidth = 0.05f; lr.endWidth = 0.05f; lr.loop = false; 
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            int ptsChute = 2; // Reta simples
            int ptsOval = (resolucaoCurva * 2) + 20;
            lr.positionCount = ptsChute + ptsOval;

            float raio = raioCurva + (r * larguraRaia);
            float perim = (2 * comprimentoReta) + (2 * Mathf.PI * raio);

            int idx = 0;
            // Desenha Chute (-Ext -> 0)
            for (int i = 0; i < ptsChute; i++) {
                float t = (float)i / ptsChute; 
                float d = Mathf.Lerp(-extensaoChute, -0.01f, t); // Vai até quase 0 para conectar visualmente
                SetLrPos(lr, idx++, d, r);
            }
            // Desenha Oval (0 -> Perim)
            for (int i = 0; i < ptsOval; i++) {
                float t = (float)i / (ptsOval - 1);
                float d = t * perim;
                SetLrPos(lr, idx++, d, r);
            }
        }
    }

    void SetLrPos(LineRenderer lr, int i, float d, int r)
    {
        // Usa a rota geométrica pura, sem wrap para o chute
        PosicaoRotacao p = CalcularRotaGeometrica(d, (float)r);
        Vector3 posLocal = transform.InverseTransformPoint(p.posicao);
        posLocal.y += 0.05f; // Altura para evitar Z-Fighting
        lr.SetPosition(i, posLocal);
    }

    // --- ROTA GEOMÉTRICA ---
    public struct PosicaoRotacao { public Vector3 posicao; public Quaternion rotacao; }

    public PosicaoRotacao CalcularRota(float distanciaPercorrida, float raiaIndexFloat)
    {
        float raioDaRaia = raioCurva + (raiaIndexFloat * larguraRaia);
        float perimetroDestaRaia = (2 * comprimentoReta) + (2 * Mathf.PI * raioDaRaia);
        
        float d = distanciaPercorrida;
        if (d >= 0) d %= perimetroDestaRaia;
        
        return CalcularRotaGeometrica(d, raiaIndexFloat);
    }

    private PosicaoRotacao CalcularRotaGeometrica(float d, float raiaIndexFloat)
    {
        Vector3 pAtual = CalcularPosicaoGeometrica(d, raiaIndexFloat);
        float deltaLook = 0.5f;
        float dFuturo = d + deltaLook;

        if (d >= 0) {
            float raio = raioCurva + (raiaIndexFloat * larguraRaia);
            float perim = (2 * comprimentoReta) + (2 * Mathf.PI * raio);
            dFuturo %= perim;
        }

        Vector3 pFuturo = CalcularPosicaoGeometrica(dFuturo, raiaIndexFloat);
        Vector3 direcao = (pFuturo - pAtual).normalized;
        if (direcao == Vector3.zero) direcao = Vector3.forward;
        Quaternion rot = Quaternion.LookRotation(direcao);
        return new PosicaoRotacao { posicao = transform.TransformPoint(pAtual), rotacao = transform.rotation * rot };
    }

    void CriarMarcaNoChao(GameObject parent, float distancia, float raiaIndex, Color cor, string nome) {
        GameObject marca = GameObject.CreatePrimitive(PrimitiveType.Quad); marca.name = nome; marca.transform.parent = parent.transform; DestroyImmediate(marca.GetComponent<Collider>());
        PosicaoRotacao pr = CalcularRota(distancia, raiaIndex);
        marca.transform.position = pr.posicao; marca.transform.rotation = pr.rotacao; marca.transform.Rotate(90, 0, 0); marca.transform.position += Vector3.up * 0.06f; marca.transform.localScale = new Vector3(larguraRaia * 0.9f, 0.15f, 1f);
        Renderer rend = marca.GetComponent<Renderer>();
        #if UNITY_EDITOR
        Material tempMat = new Material(Shader.Find("Standard")); tempMat.color = cor; rend.sharedMaterial = tempMat;
        #else
        rend.material.color = cor;
        #endif
    }
    void CriarLinhaTransversal(GameObject parent, float distancia, Color cor, string nome, bool xadrez = false) {
        GameObject linha = GameObject.CreatePrimitive(PrimitiveType.Quad); linha.name = nome; linha.transform.parent = parent.transform;
        BoxCollider col = linha.GetComponent<BoxCollider>(); if (col != null) { col.isTrigger = true; col.size = new Vector3(1f, 1f, 20f); }
        float centroPistaIndex = (float)numeroRaias / 2f; PosicaoRotacao pr = CalcularRota(distancia, centroPistaIndex);
        linha.transform.position = pr.posicao; linha.transform.rotation = pr.rotacao; linha.transform.Rotate(90, 0, 0); linha.transform.position += Vector3.up * 0.07f;
        float larguraTotal = numeroRaias * larguraRaia; linha.transform.localScale = new Vector3(larguraTotal, 0.6f, 1f);
        Renderer rend = linha.GetComponent<Renderer>(); if (xadrez && materialChegada != null) rend.sharedMaterial = materialChegada;
        else {
            #if UNITY_EDITOR
            Material tempMat = new Material(Shader.Find("Standard")); tempMat.color = cor; rend.sharedMaterial = tempMat;
            #else
            rend.material.color = cor;
            #endif
        }
    }
}