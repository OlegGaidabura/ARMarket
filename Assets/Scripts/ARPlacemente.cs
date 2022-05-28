using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacemente : MonoBehaviour
{
    private enum PlacementType
    {
        Horizontal,
        Vertical
    }

    public GameObject UICanvas;
    public GameObject CurrentCanvas;
    public GameObject GoBackButton;
    public GameObject placmentIndicator;
    private GameObject spawnedObject;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager planeManager;
    private bool placementPoseIsValid = false;
    private ARPlane plane;
    private PlacementType objectPlacementType;
    private PlaneAlignment applyAlignment;

    public GameObject[] ARModelsHorizontal;
    public GameObject[] ARModelsVertical;
    private GameObject[] ARModels;
    int modelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        planeManager = FindObjectOfType<ARPlaneManager>();
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        var menu = GameObject.FindGameObjectsWithTag("Menu"); // Находит все доступные Canvas Menu
        foreach (var obj in menu)
            obj.SetActive(false); // Выключает отображение UI, чтобы не происходило наложение
        CurrentCanvas.gameObject.SetActive(true); // Включает главное меню
        UICanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if( spawnedObject == null
        && placementPoseIsValid 
        && Input.touchCount > 0 
        && Input.GetTouch(0).phase == TouchPhase.Began
        && !CurrentCanvas.active){ // При нажатии ставит объект (если допустимо)
            ARPlaceObject();
            UICanvas.gameObject.SetActive(true);
        }
        
        if (Camera.main == null) 
            throw new System.Exception();
        UpdatePlacementPose();
        UpdatePlacementIndicator();   
    }

    // Обновляет положение индикатора (красный круг) на камере
    // Если проверка позиции проходит, то ставит индикатор
    void UpdatePlacementIndicator(){
        if(placementPoseIsValid){
            placmentIndicator.SetActive(true);
            placmentIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else {
            placmentIndicator.SetActive(false);
        }
    }

    // Проверяет допустимость установки индикатора
    // Строит плоскости
    void UpdatePlacementPose(){
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if(spawnedObject == null && placementPoseIsValid){
            PlacementPose = hits[0].pose;
            plane = planeManager.GetPlane(hits[0].trackableId);
        }

    }

    // Устанавливает 3D объект на место индикатора
    void ARPlaceObject(){
        var clearUp = GameObject.FindGameObjectWithTag("ARMultiModel");
        Destroy(clearUp);
        if (plane.alignment == PlaneAlignment.HorizontalDown || plane.alignment == PlaneAlignment.HorizontalUp)
        {
            spawnedObject = Instantiate(ARModelsHorizontal[modelIndex], PlacementPose.position, PlacementPose.rotation);
            objectPlacementType = PlacementType.Horizontal;
        }
        else
        {
            spawnedObject = Instantiate(ARModelsVertical[modelIndex], PlacementPose.position, PlacementPose.rotation);
            objectPlacementType = PlacementType.Vertical;
        }
    }

    // Сменить модель (следующая)
    public void ModelChangeRight(){
        if(objectPlacementType == PlacementType.Horizontal)
            modelIndex = modelIndex + 1 >= ARModelsHorizontal.Length ? 0 : modelIndex + 1;
        else
            modelIndex = modelIndex + 1 >= ARModelsVertical.Length ? 0 : modelIndex + 1;
        ARPlaceObject();
    }

    // Сменить модель (предыдущая)
    public void ModelChangeLeft(){
        if(objectPlacementType == PlacementType.Horizontal)
            modelIndex = modelIndex - 1 < 0 ? ARModelsHorizontal.Length - 1 : modelIndex - 1;
        else
            modelIndex = modelIndex - 1 < 0 ? ARModelsVertical.Length - 1 : modelIndex - 1;
        ARPlaceObject();
    }

    // Удалить установленный объект
    public void DeleteObject()
    {
        var clearUp = GameObject.FindGameObjectWithTag("ARMultiModel");
        Destroy(clearUp);
        modelIndex = 0;
        UICanvas.gameObject.SetActive(false);
    }

    // Перейти к следующему Canvas
    public void GoTo(GameObject nextCanvas)
    {
        CurrentCanvas.gameObject.SetActive(false);
        nextCanvas.gameObject.SetActive(true);
        CurrentCanvas = nextCanvas;
    }

    // Перейти из Canvas(Choose Plane) в камеру
    public void GoToCamera(int alignment)
    {
        CurrentCanvas.SetActive(false);
        applyAlignment = alignment == 0 ? PlaneAlignment.HorizontalUp : alignment == 1 
                                        ? PlaneAlignment.Vertical : PlaneAlignment.HorizontalDown;
        planeManager.detectionMode = applyAlignment == PlaneAlignment.Vertical ? 
            PlaneDetectionMode.Vertical 
            : PlaneDetectionMode.Horizontal;
        GoBackButton.SetActive(true);
    }

    public void ChangePlanePanel(GameObject canvas)
    {
        DeleteObject();
        GoBackButton?.SetActive(false);
        canvas.SetActive(true);
    }
}
