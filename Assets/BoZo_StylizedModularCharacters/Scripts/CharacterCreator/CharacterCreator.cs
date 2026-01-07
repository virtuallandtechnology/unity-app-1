using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using TMPro;
using System.Linq;
using VirtualLand; // Added to access ApiClient

namespace Bozo.ModularCharacters
{
    public class CharacterCreator : MonoBehaviour
    {
        [Header("Creator Dependencies")]
        public OutfitSystem character;
        [SerializeField] ColorPickerControl colorPickerControl;
        [SerializeField] CharacterSpinner Spinner;
        [SerializeField] Camera iconCamera;
        [SerializeField] RenderTexture iconTexture;
        public OutfitType[] outfitTypes;

        [Header("Outfit Dependencies")]
        private Dictionary<string, Outfit> OutfitDataBase = new Dictionary<string, Outfit>();
        [SerializeField] OutfitSelector outfitSelectorObject;
        private List<OutfitSelector> outfitSelectors = new List<OutfitSelector>();
        [SerializeField] Transform outfitContainer;

        [Header("Texture Dependencies")]
        [SerializeField] TextureSelector textureSelectorObject;
        private List<TextureSelector> textureSelectors = new List<TextureSelector>();
        [SerializeField] Transform decalContainer;
        [SerializeField] Transform patternContainer;

        [Header("BodyShape Dependencies")]
        [SerializeField] BlendSlider blendSliderObject;
        private List<BlendSlider> blendSliders = new List<BlendSlider>();
        private List<BlendSlider> faceBlendSliders = new List<BlendSlider>();

        [SerializeField] BodyShapeSliders modSliderObject;
        private List<BodyShapeSliders> ModSliders = new List<BodyShapeSliders>();

        [SerializeField] Transform bodyShapeContainer;
        [SerializeField] Transform bodyModContainer;
        [SerializeField] Transform faceShapeContainer;
        [SerializeField] Transform faceModContainer;

        [SerializeField] GameObject currentPage;
        private GameObject previousPage;

        private Dictionary<string, List<GameObject>> outfits = new Dictionary<string, List<GameObject>>();
        private List<TexturePackage> textures = new List<TexturePackage>();

        [Header("Save Dependencies")]
        [SerializeField] SaveSelector saveSelector;
        [SerializeField] Dictionary<string, SaveSelector> saveSlots = new Dictionary<string, SaveSelector>();
        [SerializeField] Transform saveContainer;
        [SerializeField] GameObject DeleteConfirmWindow;
        [SerializeField] TMP_Text loadedCharacterNameText;
        [SerializeField] TMP_Text DeleteCharacterNameText;


        private OutfitType type;

        [Header("Save Options")]
        public TMP_InputField CharacterName;

        private void Awake()
        {
            outfits.Clear();
            OutfitDataBase.Clear();

            var ob = Resources.LoadAll<Outfit>("");
            var textureObjects = Resources.LoadAll<TexturePackage>("");
            foreach (var item in ob)
            {
                if (!item.showCharacterCreator) continue;
                OutfitDataBase.Add(item.name, item.GetComponent<Outfit>());
            }
            foreach (var item in textureObjects)
            {
                textures.Add(item.GetComponent<TexturePackage>());
            }

            GenerateOutfitSelection();
            GenerateTextureSelection();
        }



        private void OnDisable()
        {
            if (character != null)
            {
                character.OnOutfitChanged -= OnOutfitUpdate;
                character.OnRigChanged -= OnRigUpdate;
            }
        }

        public void Start()
        {
            if (character != null)
            {
                character.OnOutfitChanged += OnOutfitUpdate;
                character.OnRigChanged += OnRigUpdate;
            }
            GetBodyBlends();
            GetFaceBlends();
            GetBodyMods();
            SwitchCatagory("Top");

            // Log the save path to find where files are stored locally
            Debug.Log($"[CharacterCreator] Local Save Path: {BMAC_SaveSystem.filePath}");
            UpdateCharacterSaves();

        }

        public void GenerateOutfitSelection() 
        {
            var outfits = OutfitDataBase.Values.ToArray();
            foreach (var item in outfits)
            {
                var selector = Instantiate(outfitSelectorObject, outfitContainer);
                selector.Init(item, this);
                outfitSelectors.Add(selector);
            }
        }

        public void GenerateTextureSelection()
        {
            foreach (var item in textures)
            {
                Transform container = null;

                if (item.type == TextureType.Decal) container = decalContainer;
                if (item.type == TextureType.Pattern) container = patternContainer;

                var selector = Instantiate(textureSelectorObject, container);
                selector.Init(item, this);
                textureSelectors.Add(selector);
            }
        }

        public void GetBodyBlends()
        {
            if (character == null) return;
            for (int i = 0; i < blendSliders.Count; i++)
            {
                Destroy(blendSliders[i].gameObject);
            }
            blendSliders.Clear();

            var shapes = character.GetShapes();
            foreach (var item in shapes)
            {
                var blendSlider = Instantiate(blendSliderObject, bodyShapeContainer);
                blendSlider.Init(character, item);
                blendSliders.Add(blendSlider);
            }
        }

        public void GetFaceBlends()
        {
            if (character == null) return;
            for (int i = 0; i < faceBlendSliders.Count; i++)
            {
                Destroy(faceBlendSliders[i].gameObject);
            }
            faceBlendSliders.Clear();

            var shapes = character.GetFaceShapes();
            foreach (var item in shapes)
            {
                var blendSlider = Instantiate(blendSliderObject, faceShapeContainer);
                blendSlider.Init(character, item);
                faceBlendSliders.Add(blendSlider);
            }
        }

        public void GetBodyMods()
        {
            if (character == null) return;
            var mods = character.GetMods().Values.ToList();

            for (int i = 0; i < ModSliders.Count; i++)
            {
                Destroy(ModSliders[i].gameObject);
            }
            ModSliders.Clear();

            var container = bodyShapeContainer;
            foreach (var item in mods)
            {
                if (item.sorting == "Head") container = faceModContainer;
                else container = bodyModContainer;
                var blendSlider = Instantiate(modSliderObject, container);
                blendSlider.Init(character, item);
                ModSliders.Add(blendSlider);
            }
        }

        public void UpdateCharacterSaves()
        {
            var saves = saveSlots.Values.ToArray();
            foreach (var item in saves)
            {
                Destroy(item.gameObject);
            }
            saveSlots.Clear();

            if (!System.IO.Directory.Exists(BMAC_SaveSystem.filePath))
            {
                System.IO.Directory.CreateDirectory(BMAC_SaveSystem.filePath);
                print("Created Save JSON save Location At: " + BMAC_SaveSystem.filePath);
            }
            if (!System.IO.Directory.Exists(BMAC_SaveSystem.iconFilePath))
            {
                System.IO.Directory.CreateDirectory(BMAC_SaveSystem.iconFilePath);
                print("Created Icon save Location At: " + BMAC_SaveSystem.iconFilePath);
            }

            string path = BMAC_SaveSystem.filePath;
            string[] jsonFiles = System.IO.Directory.GetFiles(path, "*.json");
            string[] icons = System.IO.Directory.GetFiles(BMAC_SaveSystem.iconFilePath, "*.png");

            for (int i = 0; i < jsonFiles.Length; i++)
            {
                var json = System.IO.File.ReadAllText(jsonFiles[i]);
                var data = JsonUtility.FromJson<CharacterData>(json);
                var image = System.IO.File.ReadAllBytes(icons[i]);
                var texture = new Texture2D(2, 2);
                Sprite icon = null;
                if (texture.LoadImage(image))
                {
                    icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }

                var selector = Instantiate(saveSelector, saveContainer);
                selector.Init(data, icon, this);
                saveSlots.Add(data.characterName, selector);
            }

            var saveObjects = Resources.LoadAll<CharacterObject>("");

            for (int i = 0; i < saveObjects.Length; i++)
            {
                var ob = saveObjects[i];
                if (saveSlots.ContainsKey(ob.data.characterName))
                {
                    continue;
                }
                else
                {
                    var selector = Instantiate(saveSelector, saveContainer);
                    Sprite icon = null;
                    if(ob.icon != null)
                    {
                        icon = Sprite.Create(ob.icon, new Rect(0, 0, ob.icon.width, ob.icon.height), new Vector2(0.5f, 0.5f));
                    }
                    selector.Init(ob.data, icon, this);
                    saveSlots.Add(ob.data.characterName, selector);
                }
            }
        }
        public void OpenPage(GameObject page) 
        {
            currentPage.SetActive(false);
            previousPage = currentPage;
            currentPage = page;
            currentPage.SetActive(true);
        }

        public void BackPage() 
        {
            currentPage.SetActive(false);
            currentPage = previousPage;
            currentPage.SetActive(true);
            colorPickerControl.RemoveObject();
        }

        public void SetOutfit(Outfit outfit) 
        {
            if (character == null) return;
            var inst = Instantiate(outfit, character.transform);
            SetColorPickerObject(inst);
            SwitchTextureCatagory(outfit.TextureCatagory);
            type = outfit.Type;

           // var type = (OutfitType)Enum.Parse(typeof(OutfitType), catagory);
        }

        public void OnOutfitUpdate(Outfit outfit)
        {
            if (outfit == null) return;
            if(outfit.Type.name == "Head")
            {
                GetFaceBlends();
            }
            if (outfit.Type.name == "Body")
            {
                GetBodyBlends();
            }
        }

        public void OnRigUpdate(SkinnedMeshRenderer rig)
        {
            //GetBodyBlends();
            GetBodyMods();
        }

        public void SetOutfitDecal(Texture texture)
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetDecal(texture);
        }
        public void SetOutfitPattern(Texture texture)
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetPattern(texture);
        }

        public void RemoveOutfit()
        {
            if (character == null) return;
            if (type == null) return;
            character.RemoveOutfit(type, true);
            colorPickerControl.RemoveObject();
        }

        public void RemoveOutfitDecal() 
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetDecal(null);
        }

        public void RemoveOutfitPattern()
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetPattern(null);
        }


        public void SwitchCatagory(string catagory) 
        {
            if (character == null) return;
            foreach (var item in outfitSelectors)
            {
                item.SetVisable(catagory);
            }

            var outfit = character.GetOutfit(catagory);
            
            SetColorPickerObject(outfit);

            if (outfit == null) return;

            SwitchTextureCatagory(outfit.TextureCatagory);
            type = outfit.Type;
        }

        public void SwitchTextureCatagory(string catagory)
        {
            if (catagory == "")
            {
                catagory = "Outfit";
            }

            foreach (var item in textureSelectors)
            {
                item.SetVisable(catagory);
            }
        }

        public void SetColorPickerObject(string type)
        {
            if (character == null) return;
            if (colorPickerControl == null) return; // Safety check
            var outfit = character.GetOutfit(type);
            colorPickerControl.ChangeObject(outfit);
        }

        public void SetColorPickerObject(Outfit outfit)
        {
            if (colorPickerControl == null) return; // Safety check
            colorPickerControl.ChangeObject(outfit);
        }

        public void ReplaceCharacter(OutfitSystem character)
        {
            if (this.character != null)
            {
                this.character.OnOutfitChanged -= OnOutfitUpdate;
                this.character.OnRigChanged -= OnRigUpdate;
                Destroy(this.character.gameObject);
            }
            this.character = character;
            if (this.character != null)
            {
                this.character.OnOutfitChanged += OnOutfitUpdate;
                this.character.OnRigChanged += OnRigUpdate;
            }
            if (Spinner != null)
            {
                Spinner.SetCharacter(character.transform);
            }
        }

        public void GetCurrentCatagory()
        {
            if (type == null) return;
            SwitchCatagory(type.name);
        }

        public void CopyColor(string copyTo)
        {
            if (character == null) return;
            var copyOutfit = character.GetOutfit((OutfitType)Enum.Parse(typeof(OutfitType), copyTo));
            colorPickerControl.CopyColor(copyOutfit);
        }

        public void ToggleWalk(bool value)
        {
            if (character == null) return;
            character.animator.SetBool("isWalk", value);
        }

        public void SaveCharacter()
        {
            StartCoroutine(Save());
        }

        public void SaveCharacter(int avatarId, string name, Action onComplete = null)
        {
            if (CharacterName != null) CharacterName.text = name;
            StartCoroutine(Save(avatarId, onComplete));
        }

        public Outfit GetOutfit(string outfitName)
        {
            return OutfitDataBase[outfitName];
        }

        [ContextMenu("Save")]
        private IEnumerator Save(int avatarId = -1, Action onComplete = null)
        {
            if (character == null) yield break;
            yield return new WaitForEndOfFrame();
            if(CharacterName.text.Length == 0)
            {
                Debug.LogWarning("Please enter in a name with at least one letter");
                yield break;
            }

            RenderTexture.active = iconTexture;
            Texture2D icon = new Texture2D(iconTexture.width, iconTexture.height, TextureFormat.RGBA32, false);
            Rect rect = new Rect(new Rect(0, 0, iconTexture.width, iconTexture.height));
            icon.ReadPixels(rect, 0,0);
            icon.Apply();

            byte[] bytes = icon.EncodeToPNG();

            Texture2D characterIcon = null;

#if UNITY_EDITOR

            System.IO.File.WriteAllBytes(Application.dataPath + BMAC_SaveSystem.iconAssetPath + CharacterName.text + ".png", bytes);
            AssetDatabase.Refresh();

            characterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/" + BMAC_SaveSystem.iconAssetPath + CharacterName.text + ".png");
            print(BMAC_SaveSystem.iconAssetPath + CharacterName.text + ".png");
            TextureImporter importer = AssetImporter.GetAtPath("Assets/" + BMAC_SaveSystem.iconAssetPath + CharacterName.text + ".png") as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
#endif

            if(!System.IO.Directory.Exists(BMAC_SaveSystem.iconFilePath))
            {
                System.IO.Directory.CreateDirectory(BMAC_SaveSystem.iconFilePath);
            }

            System.IO.File.WriteAllBytes(BMAC_SaveSystem.iconFilePath + "/" + CharacterName.text + ".png", bytes);
            BMAC_SaveSystem.SaveCharacter(character, CharacterName.text, characterIcon);

            UpdateCharacterSaves();

            if (avatarId != -1)
            {
                // Construct API JSON
                // Get full character data
                var data = BMAC_SaveSystem.GetCharacterData(character);
                
                // Optimized Data: Remove Position and Rotation info to reduce size
                if (data.bodyMods != null)
                {
                    foreach (var mod in data.bodyMods)
                    {
                        mod.posValue = 0;
                        mod.position = Vector3.zero;
                        mod.rotation = 0;
                    }
                }

                // Create filtered DTO
                var saveData = new CharacterSaveData
                {
                    characterName = data.characterName,
                    bodyIDs = data.bodyIDs,
                    bodyShapes = data.bodyShapes,
                    faceIDs = data.faceIDs,
                    faceShapes = data.faceShapes,
                    bodyModsKeys = data.bodyModsKeys,
                    bodyMods = data.bodyMods,
                    outfitDatas = data.outfitDatas
                };
                
                
                // Serialize saveData using JsonUtility (handles Unity types properly)
                string styleJson = JsonUtility.ToJson(saveData);

                // Construct payload with avatar_id and style as STRING (not object)
                var payload = new
                {
                    avatar_id = avatarId.ToString(),
                    style = styleJson  // Keep as string
                };

                // Serialize the payload
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                // Use avatarId (product ID) as the key instead of name
                string key = avatarId.ToString();

                Debug.Log($"[CharacterCreator] ========== SAVE CHARACTER ==========");
                Debug.Log($"[CharacterCreator] Product ID (Key): {key}");
                Debug.Log($"[CharacterCreator] Character Name: {name}");
                Debug.Log($"[CharacterCreator] API Endpoint: /user/profile/update/{key}");
                Debug.Log($"[CharacterCreator] Payload: {json}");
                Debug.Log($"[CharacterCreator] Style JSON Length: {styleJson.Length} characters");
                Debug.Log($"[CharacterCreator] =====================================");

                ApiClient.Get().UpdateProfileData(key, json,
                    (res) =>
                    {
                        Debug.Log($"[CharacterCreator] ✓ Profile updated successfully for Product ID '{key}'");
                        onComplete?.Invoke();
                    },
                    (err) =>
                    {
                        Debug.LogError($"[CharacterCreator] ✗ Failed to update profile for Product ID '{key}': {err}");
                        onComplete?.Invoke();
                    });
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private int GetOutfitIndex(string category)
        {
            if (character == null) return 0;
            var currentOutfit = character.GetOutfit(category);
            if (currentOutfit == null) return 0;

            // Find index in OutfitDataBase or Resources
            // We use the list of all outfits of this type to determine index
            var allOutfits = Resources.LoadAll<Outfit>("");
            int index = 0;
            int matchIndex = 0;

            foreach (var outfit in allOutfits)
            {
                if (outfit.Type != null && outfit.Type.name == category)
                {
                    // Check if names match (ignoring (Clone))
                    if (outfit.name == currentOutfit.name.Replace("(Clone)", ""))
                    {
                        matchIndex = index;
                    }
                    index++;
                }
            }
            return matchIndex;
        }

        public void LoadCharacter(CharacterData data)
        {
            if (character == null) return;
            loadedCharacterNameText.text = data.characterName;
            _ = BMAC_SaveSystem.LoadCharacter(character, data);
        }

        public void DeleteCharacter()
        {
            if (loadedCharacterNameText.text == "") return;
            DeleteCharacterNameText.text = "Delete: " + loadedCharacterNameText.text;
            DeleteConfirmWindow.SetActive(true);
        }

        public void ConfirmDelete()
        {
            BMAC_SaveSystem.DeleteCharacter(loadedCharacterNameText.text);
            loadedCharacterNameText.text = "";
            UpdateCharacterSaves();
        }

        [Serializable]
        public class CharacterSaveData
        {
            public string characterName;
            public List<string> bodyIDs;
            public List<float> bodyShapes;
            public List<string> faceIDs;
            public List<float> faceShapes;
            public List<string> bodyModsKeys;
            public List<BodyModData> bodyMods;
            public List<OutfitData> outfitDatas;
        }

    }
}
