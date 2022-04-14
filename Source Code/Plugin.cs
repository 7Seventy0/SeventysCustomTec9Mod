using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Utilla;

namespace SeventysCustomTec9Mod
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        bool modOn;
        GameObject bullet;
        GameObject spawnPoint;
        bool rightTrigger;
        float countdown;
        private readonly XRNode rNode = XRNode.RightHand;
        GameObject tec9Instance;
        GameObject tec9;
        GameObject sPInstance;
        GameObject fireParticle;
        GameObject firesoundGameobject;
        GameObject tecBody;
        void OnEnable()
        {
            
            modOn = true;
            

            HarmonyPatches.ApplyHarmonyPatches();
            Utilla.Events.GameInitialized += OnGameInitialized;
            if(tec9Instance == null)
            {
                SeventysStart();
            }
            if(tec9Instance.transform.parent == null)
            {
                AttachModel();
            }

           
            
        }
        public static ConfigEntry<float> rateOfFire;
        public static ConfigEntry<float> bulletspeed;
        public static ConfigEntry<float> bulletScale;
        public static ConfigEntry<float> fireAudioVolume;
        void Start()
        {
            ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "SeventysTec9.cfg"), true);
            rateOfFire = config.Bind<float>("Config", "Rate of Fire", 0.15f, "Lower = faster gun!");
            bulletspeed = config.Bind<float>("Config", "Bullet Travel Speed", 50f, "Higher = faster bullet!");
            bulletScale = config.Bind<float>("Config", "Scale of Bullet", 0.03f, "Higer = bigger bullet!");
            fireAudioVolume = config.Bind<float>("Config", "Volume of Fire sound effect", 0.1f, "Higer = Louder");
            
        }
        void SeventysStart()
        {
            if (modOn)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bullet.AddComponent<Rigidbody>();
                bullet.transform.localScale = new Vector3(bulletScale.Value, bulletScale.Value, bulletScale.Value);
                bullet.GetComponent<SphereCollider>().enabled = false;
                bullet.layer = 18;
                bullet.AddComponent<Rigidbody>();

                Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("SeventysCustomTec9Mod.Assets.seventystec9");
                Stream str1 = Assembly.GetExecutingAssembly().GetManifestResourceStream("SeventysCustomTec9Mod.Assets.tec9particlebundle");
                Stream str2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("SeventysCustomTec9Mod.Assets.tec9audiobundle");

                AssetBundle bundle = AssetBundle.LoadFromStream(str);
                AssetBundle bundle1 = AssetBundle.LoadFromStream(str1);
                AssetBundle bundle2 = AssetBundle.LoadFromStream(str2);

                tec9 = bundle.LoadAsset<GameObject>("tec9");
                fireParticle = bundle1.LoadAsset<GameObject>("fireeffect");
                fireParticle.AddComponent<DestroyParticle>();
                firesoundGameobject = bundle2.LoadAsset<GameObject>("audioGameObject");
                firesoundGameobject.GetComponent<AudioSource>().volume = fireAudioVolume.Value;
                firesoundGameobject.AddComponent<DestroyParticle>();

                Instantiate(tec9);
                Instantiate(fireParticle);

                if (bundle == null)
                {
                    Debug.Log("Failed to load AssetBundle!");
                    return;
                }

                spawnPoint = new GameObject("Bullet spawn point");
                sPInstance = Instantiate(spawnPoint);
                if (modOn)
                {
                    AttachModel();
                }
            }

        }
        void AttachModel()
        {
            GameObject hand = GameObject.Find("palm.01.R");




            tec9Instance = GameObject.Find("tec9(Clone)");
            tec9Instance.transform.SetParent(hand.transform, false);
            tec9Instance.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            tec9Instance.transform.localPosition = new Vector3(0f, 0.1f, 0);
            tec9Instance.transform.localEulerAngles = new Vector3(11.0817f, 57.3376f, 289.5621f);
            tecBody = GameObject.Find("tec9(Clone)/Body");
            sPInstance.transform.SetParent(tecBody.transform, false);
            sPInstance.transform.localEulerAngles = new Vector3(0, 0, 0);
            sPInstance.transform.localPosition = new Vector3(-0.4f, 0.07f, 0);
        }
        void OnDisable()
        {
            modOn = false;
            

            HarmonyPatches.RemoveHarmonyPatches();
            Utilla.Events.GameInitialized -= OnGameInitialized;
            if(tec9Instance != null)
            {
                tec9Instance.transform.SetParent(null);
                tec9Instance.transform.localPosition = Vector3.zero;
            }
        }


        void OnGameInitialized(object sender, EventArgs e)
        {
            SeventysStart();

        }
        void Update()
        {
            if (modOn)
            {
                InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out rightTrigger);

                if (rightTrigger)
                {

                    countdown -= Time.deltaTime;
                    if (countdown <= 0)
                    {
                        Shot();
                        countdown = rateOfFire.Value;
                    }

                    Debug.Log(countdown);
                }
                else
                {
                 
                }
                if (Keyboard.current.lKey.isPressed)
                {


                    countdown -= Time.deltaTime;
                    if (countdown <= 0)
                    {
                        Shot();
                        countdown = rateOfFire.Value;
                    }

                    Debug.Log(countdown);
                }

            }
        }
        void Shot()
        {
            Debug.Log("Boom");
            GameObject gamerBullet = Instantiate(bullet, sPInstance.transform.position, sPInstance.transform.rotation) ;
            gamerBullet.AddComponent<DestroyParticle>();
            Instantiate(fireParticle, sPInstance.transform.position, sPInstance.transform.rotation);
            gamerBullet.GetComponent<Rigidbody>().AddRelativeForce(Vector3.left * bulletspeed.Value, ForceMode.Impulse);
            Instantiate(firesoundGameobject, sPInstance.transform.position, sPInstance.transform.rotation);
        }

        /* This attribute tells Utilla to call this method when a modded room is joined */
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            /* Activate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = true;
        }

        /* This attribute tells Utilla to call this method when a modded room is left */
        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            /* Deactivate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = false;
        }
    }
}
