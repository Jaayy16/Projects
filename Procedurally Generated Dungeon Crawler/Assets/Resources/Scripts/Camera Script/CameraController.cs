using ProceduralDungeon.Combat;
using ProceduralDungeon.Generator;
using ProceduralDungeon.Settings;
using UnityEngine;
using ProceduralDungeon.Player;

public class CameraController : MonoBehaviour
{
    private Transform playerTransform;
    private Camera mainCamera;
    
    [Header("Camera Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Biome Tint Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color floodedColor = new Color(0.3f, 0.5f, 1f, 1f);
    [SerializeField] private Color moltenColor = new Color(1f, 0.3f, 0.1f, 1f);
    
    [SerializeField, Range(0, 1)] private float colorIntensity = 0.3f;
    [SerializeField] private float transitionSpeed = 5f;
    
    private Color targetColor;
    private Color currentColor;
    private BiomeType currentBiome =  BiomeType.Normal;
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        currentColor = normalColor;
        targetColor = normalColor;
        UpdateCameraColor();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    void LateUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 desiredPosition = playerTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        UpdateBiomeTint();
        
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        UpdateCameraColor();
    }
    
    public void SetPlayerTransform(Transform playerTrans)
    {
        playerTransform = playerTrans;
    }

    private void UpdateBiomeTint()
    {
        if (playerTransform == null)
        {
            return;
        }
        
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();

        if (playerController == null)
        {
            return;
        }
        
        BiomeType detectedBiome = playerController.GetCurrentBiome();

        if (detectedBiome != currentBiome)
        {
            currentBiome = detectedBiome;
            SetBiomeTint(currentBiome);
        }
    }

    public void SetBiomeTint(BiomeType biomeType)
    {
        switch (biomeType)
        {
            case BiomeType.Normal:
                targetColor = normalColor;
                break;
            case BiomeType.Flooded:
                targetColor = floodedColor;
                break;
            case BiomeType.Molten:
                targetColor = moltenColor;
                break;
            default:
                targetColor = normalColor;
                break;
        }
    }

    private void UpdateCameraColor()
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = currentColor;
        }
    }
}
