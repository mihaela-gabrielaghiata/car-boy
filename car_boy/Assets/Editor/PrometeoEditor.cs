using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(PrometeoCarController))]
[System.Serializable]
public class PrometeoEditor : Editor{

  enum displayFieldType {DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields}
  displayFieldType DisplayFieldType;

  private PrometeoCarController prometeo;
  private SerializedObject SO;
  //
  //
  //CAR SETUP
  //
  //
  private SerializedProperty maxSpeed;
  private SerializedProperty maxReverseSpeed;
  private SerializedProperty accelerationMultiplier;
  private SerializedProperty maxSteeringAngle;
  private SerializedProperty steeringSpeed;
  private SerializedProperty brakeForce;
  private SerializedProperty decelerationMultiplier;
  private SerializedProperty handbrakeDriftMultiplier;
  private SerializedProperty bodyMassCenter;
  //
  //
  //WHEELS VARIABLES
  //
  //
  private SerializedProperty frontLeftMesh;
  private SerializedProperty frontLeftCollider;
  private SerializedProperty frontRightMesh;
  private SerializedProperty frontRightCollider;
  private SerializedProperty rearLeftMesh;
  private SerializedProperty rearLeftCollider;
  private SerializedProperty rearRightMesh;
  private SerializedProperty rearRightCollider;
  //
  //
  //PARTICLE SYSTEMS' VARIABLES
  //
  //
  private SerializedProperty useEffects;
  private SerializedProperty RLWParticleSystem;
  private SerializedProperty RRWParticleSystem;
  private SerializedProperty RLWTireSkid;
  private SerializedProperty RRWTireSkid;
  //
  //
  //SPEED TEXT (UI) VARIABLES
  //
  //
  private SerializedProperty useSounds;
  private SerializedProperty carEngineSound;
  private SerializedProperty tireScreechSound;


  private void OnEnable(){
    prometeo = (PrometeoCarController)target;
    SO = new SerializedObject(target);

    maxSpeed = SO.FindProperty("maxSpeed");
    maxReverseSpeed = SO.FindProperty("maxReverseSpeed");
    accelerationMultiplier = SO.FindProperty("accelerationMultiplier");
    maxSteeringAngle = SO.FindProperty("maxSteeringAngle");
    steeringSpeed = SO.FindProperty("steeringSpeed");
    brakeForce = SO.FindProperty("brakeForce");
    decelerationMultiplier = SO.FindProperty("decelerationMultiplier");
    handbrakeDriftMultiplier = SO.FindProperty("handbrakeDriftMultiplier");
    bodyMassCenter = SO.FindProperty("bodyMassCenter");

    frontLeftMesh = SO.FindProperty("frontLeftMesh");
    frontLeftCollider = SO.FindProperty("frontLeftCollider");
    frontRightMesh = SO.FindProperty("frontRightMesh");
    frontRightCollider = SO.FindProperty("frontRightCollider");
    rearLeftMesh = SO.FindProperty("rearLeftMesh");
    rearLeftCollider = SO.FindProperty("rearLeftCollider");
    rearRightMesh = SO.FindProperty("rearRightMesh");
    rearRightCollider = SO.FindProperty("rearRightCollider");

    useEffects = SO.FindProperty("useEffects");
    RLWParticleSystem = SO.FindProperty("RLWParticleSystem");
    RRWParticleSystem = SO.FindProperty("RRWParticleSystem");
    RLWTireSkid = SO.FindProperty("RLWTireSkid");
    RRWTireSkid = SO.FindProperty("RRWTireSkid");

    useSounds = SO.FindProperty("useSounds");
    carEngineSound = SO.FindProperty("carEngineSound");
    tireScreechSound = SO.FindProperty("tireScreechSound");

  }

}
