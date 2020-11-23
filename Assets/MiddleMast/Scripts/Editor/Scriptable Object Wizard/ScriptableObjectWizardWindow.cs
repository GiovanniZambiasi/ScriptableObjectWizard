// Created by Giovanni Tramarin Zambiasi
// giovanni.zambiasi@gmail.com
// 22/11/2020

using System;
using System.Collections.Generic;
using System.Reflection;
using MiddleMast.Utilities.Extensions;
using UnityEditor;
using UnityEngine;

namespace MiddleMast.Editor.ScriptableObjectWizard
{
    public class ScriptableObjectWizardWindow : EditorWindow
    {
        const string AssembliesLabel = "Assemblies to include:"; 
        
        class AssemblyConfig : IComparable<AssemblyConfig>
        {
            public Assembly assembly;
            public System.Type[] scriptableObjectTypes;
            public bool isEnabled;

            public AssemblyConfig(Assembly assembly, System.Type[] scriptableObjectTypes)
            {
                this.assembly = assembly;
                this.scriptableObjectTypes = scriptableObjectTypes;
                isEnabled = false;
            }

            public int CompareTo(AssemblyConfig other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return assembly.FullName.CompareTo(other.assembly.FullName);
            }
        }
    
        [MenuItem("MiddleMast/ScriptableObject Wizard")]
        static void Create()
        {
            var window = EditorWindow.GetWindow<ScriptableObjectWizardWindow>(false, "ScriptableObject Wizard");
            window.Show();
        }

        List<System.Type> _activeTypes = new List<Type>(128);
        List<AssemblyConfig> _assemblyConfigs = new List<AssemblyConfig>(128);
        GUILayoutOption[] _areaOptions;
        GUILayoutOption[] _typeLabelOptions;
        Vector2 _assembliesScrollPosition;
        Vector2 _typesScrollPosition;
        string _searchFilter = string.Empty;
        bool _showAssemblyOptions = true;
        bool _showTypeOptions  = true;
        
        void Awake()
        {
            var __position = position;
            __position.width = 1240;
            position = __position;
            
            _areaOptions = new[]
            {
                GUILayout.Width(__position.width / 2),
            };

            _typeLabelOptions = new[]
            {
                GUILayout.MaxWidth(246f),
            };
            
            FindAllConcreteScriptableObjectTypes();
        }

        void OnGUI()
        {
            DrawHeader();

            var __isInvalid = _assemblyConfigs == null || _assemblyConfigs.Count == 0;
            if (__isInvalid)
            {
                EditorGUILayout.HelpBox("Something is wrong. Please refresh", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                DrawAssemblyPanelIfEnabled();
                
                DrawCreationPanelfEnabled();
                
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawHeader()
        {
            EditorGUILayout.LabelField("Scriptable Object Wizard", EditorStyles.whiteLargeLabel);

            EditorGUILayout.Space(12);

            if (GUILayout.Button("Refresh Assemblies", GUILayout.MaxWidth(124f)))
            {
                FindAllConcreteScriptableObjectTypes();
            }

            EditorGUILayout.Space(12);
        }

        bool MatchesSearchCriteria(Type type)
            => _searchFilter == string.Empty || type.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);

        void CreateAsset(System.Type type)
        {
            var __pathAndName = EditorUtility.SaveFilePanelInProject($"Create {type.Name}", type.Name, "asset", "");
            
            var __object = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(__object, __pathAndName);

            Selection.activeObject = __object;
        }

        void DrawCreateOptionForType(System.Type type)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"{type.Name}", _typeLabelOptions);
            
            if (GUILayout.Button("Create"))
            {
                CreateAsset(type);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawAssemblyOption(AssemblyConfig assemblyConfig)
        {
            var __wasEnabled = assemblyConfig.isEnabled;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{assemblyConfig.assembly.GetName().Name}");
            assemblyConfig.isEnabled = EditorGUILayout.Toggle($"[{assemblyConfig.scriptableObjectTypes.Length.ToString()}] SO derived types", assemblyConfig.isEnabled);
            GUILayout.EndHorizontal();
            
            var __isEnabled = assemblyConfig.isEnabled;

            if (__wasEnabled != __isEnabled)
            {
                if (__isEnabled)
                    ActivateAssembly(assemblyConfig);
                else
                    DeactivateAssembly(assemblyConfig);
            }
        }

        void DeactivateAssembly(AssemblyConfig assemblyConfig)
        {
            var __types = assemblyConfig.scriptableObjectTypes;
            for (var __i = 0; __i < __types.Length; __i++)
            {
                var __type = __types[__i];
                _activeTypes.Remove(__type);
            }
        }

        void ActivateAssembly(AssemblyConfig assemblyConfig)
        {
            _activeTypes.AddRange(assemblyConfig.scriptableObjectTypes);
        }

        void SetAllAssemblies(bool isEnabled)
        {
            for (var __i = 0; __i < _assemblyConfigs.Count; __i++)
            {
                var __assembly = _assemblyConfigs[__i];
                __assembly.isEnabled = isEnabled;
                if(isEnabled)
                    ActivateAssembly(__assembly);
                else
                    DeactivateAssembly(__assembly);
            }
        }
        
        void FindAllConcreteScriptableObjectTypes()
        {
            var __assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            
            _assemblyConfigs.Clear();
            
            var __scriptableObjectType = typeof(ScriptableObject);
            
            for (var __i = 0; __i < __assemblies.Length; __i++)
            {
                _activeTypes.Clear();
                
                var __assembly = __assemblies[__i];
                
                __scriptableObjectType.GetAllConcreteInheritors(__assembly, _activeTypes);

                if (_activeTypes.Count == 0) 
                    continue;
                
                _assemblyConfigs.Add(new AssemblyConfig(__assembly, _activeTypes.ToArray()));
            }
            
            _assemblyConfigs.Sort();
            
            _activeTypes.Clear();
        }

        void DrawCreationPanelfEnabled()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, _areaOptions);
            
            _showTypeOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_showTypeOptions, "Available Types:");
            
            if (_showTypeOptions)
            {
               DrawCreationPanel();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.EndVertical();
        }

        void DrawCreationPanel()
        {
            if (_activeTypes.Count > 0)
            {
                EditorGUI.indentLevel++;
                
                DrawSearchBar();
                
                _typesScrollPosition = EditorGUILayout.BeginScrollView(_typesScrollPosition, false, false);

                DrawCreationOptions();

                EditorGUILayout.EndScrollView();
                
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox($"No ScriptableObjects available to create in active assemblies. Try enabling different assemblies via the \"{AssembliesLabel}\" menu above", MessageType.Info);
            }
        }

        void DrawCreationOptions()
        {
            for (int __i = 0; __i < _activeTypes.Count; __i++)
            {
                var __type = _activeTypes[__i];

                if (MatchesSearchCriteria(__type))
                    DrawCreateOptionForType(__type);
            }
        }

        void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Search:", GUILayout.Width(58f));
            
            _searchFilter = EditorGUILayout.TextField(_searchFilter);

            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                _searchFilter = string.Empty;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawAssemblyPanelIfEnabled()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, _areaOptions);
            
            _showAssemblyOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_showAssemblyOptions, AssembliesLabel);

            if (_showAssemblyOptions)
            {
                DrawAssemblyPanel();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.EndVertical();
        }

        void DrawAssemblyPanel()
        {
            EditorGUI.indentLevel++;

            _assembliesScrollPosition = EditorGUILayout.BeginScrollView(_assembliesScrollPosition, false, false);

            for (var __i = 0; __i < _assemblyConfigs.Count; __i++)
            {
                var __assemblyData = _assemblyConfigs[__i];

                DrawAssemblyOption(__assemblyData);
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Activate all", GUILayout.Width(128f)))
            {
                SetAllAssemblies(true);
            }
            if (GUILayout.Button("Deactivate all", GUILayout.Width(128f)))
            {
                SetAllAssemblies(false);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
    }
}