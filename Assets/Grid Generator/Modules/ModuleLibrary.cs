using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Grid_Generator.Modules
{
    [CreateAssetMenu(menuName = "ScriptableObject/ModuleLibrary")]
    public class ModuleLibrary : SerializedScriptableObject
    {
        [SerializeField, Tooltip("导入的模型")] private GameObject importedModules;

        // 波函数坍缩需要List<Module>
        [OdinSerialize] public Dictionary<string, List<Module>> moduleLibrary = new Dictionary<string, List<Module>>();
        
        [Button]
        public void ImportModule()
        {
            for (var i = 1; i < 256; i++) // 初始化字典，将十进制转化为二进制作为key存入
            {
                moduleLibrary.Add(Convert.ToString(i, 2).PadLeft(8, '0'), new List<Module>());
            }

            // 将所有模块导入dictionary，这里不需要任何旋转或者镜像
            foreach (Transform child in importedModules.transform)
            {
                var mesh = child.GetComponent<MeshFilter>().sharedMesh;
                var moduleName = child.name;
                moduleLibrary[moduleName].Add(new Module(moduleName, mesh, 0, false));

                // 我们需要通过对模块name的判断，来判断该导入的模块模型是否存在旋转或者镜像的新模型
                // 如果不存在则导入旋转九十度之后得到的新模块
                if (!RotateEqualCheck(moduleName))
                {
                    var newName = RotateName(moduleName, 1);
                    moduleLibrary[newName].Add(new Module(newName, mesh, 1, false));
                    // 在初始模块旋转90度和自己不同的基础上判断初始模块旋转180度之后是否与自己相同
                    if (!RotateTwiceEqualCheck(moduleName))
                    {
                        var newName2 = RotateName(moduleName, 2);
                        moduleLibrary[newName2].Add(new Module(newName2, mesh, 2, false));
                        var newName3 = RotateName(moduleName, 3);
                        moduleLibrary[newName3].Add(new Module(newName3, mesh, 3, false));
                        // 在初始模块旋转90度和180度都和自己不同的基础上，再判断初始模块的镜像是否与自己相同
                        // 如果镜像后和自己不同，则镜像模块并旋转90°
                        if (!FlipRotationEqualCheck(moduleName))
                        {
                            moduleLibrary[FlipName(moduleName)].Add(new Module(FlipName(moduleName), mesh, 0, true));
                            moduleLibrary[RotateName(FlipName(moduleName), 1)].Add(new Module(RotateName(FlipName(moduleName), 1), mesh, 1, true));
                            moduleLibrary[RotateName(FlipName(moduleName), 2)].Add(new Module(RotateName(FlipName(moduleName), 2), mesh, 2, true));
                            moduleLibrary[RotateName(FlipName(moduleName), 3)].Add(new Module(RotateName(FlipName(moduleName), 3), mesh, 3, true));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 旋转编号name
        /// 每一次旋转其实就是将name的第四位和第八位放到第一位和第五位，其余位置顺延
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="time">旋转次数</param>
        /// <returns></returns>
        private string RotateName(string moduleName, int time)
        {
            var result = moduleName;
            // 计算等效旋转次数（4次一循环）
            time = (time % 4 + 4) % 4; // 处理负旋转
            for (var i = 0; i < time; i++)
            {
                result = result[3] + result[..3] + result[7] + result.Substring(4, 3);
            }

            return result;
        }

        /// <summary>
        /// 镜像编号
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        private string FlipName(string moduleName) =>
            moduleName[3].ToString() + moduleName[2] + moduleName[1] + moduleName[0]
            + moduleName[7] + moduleName[6] + moduleName[5] + moduleName[4];


        /// <summary>
        /// 根据二进制编号，判断模块旋转90度之后是否和自己相同，如果不相同则需要导入旋转后的新模块
        /// 判断的方法就是如果name的前四位，后四位都相同，后四位也相同，则模块旋转90度之后和自己相同
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        private bool RotateEqualCheck(string moduleName)
        {
            return moduleName.Length >= 8
                   && moduleName.Take(4).All(c => c == moduleName[0]) // 检查前四位相同
                   && moduleName.Skip(4).Take(4).All(c => c == moduleName[4]); // 检查后四位相同
        }

        private bool RotateTwiceEqualCheck(string moduleName)
        {
            return moduleName[0] == moduleName[2] && moduleName[1] == moduleName[3] && moduleName[4] == moduleName[6] && moduleName[5] == moduleName[7];
        }

        /// <summary>
        /// 判断初始模块的镜像是否与自己相同
        /// 这里的镜像包括横轴，纵轴，两个对角线为轴四种情况，一一列举出来并逐个比较
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        private bool FlipRotationEqualCheck(string moduleName)
        {
            var symmetryVertical = moduleName[3].ToString() + moduleName[2] + moduleName[1] + moduleName[0] + moduleName[7] + moduleName[6] + moduleName[5] + moduleName[4];

            var symmetryHorizontal = moduleName[1].ToString() + moduleName[0] + moduleName[3] + moduleName[2] + moduleName[5] + moduleName[4] + moduleName[7] + moduleName[6];

            var symmetry02 = moduleName[0].ToString() + moduleName[3] + moduleName[2] + moduleName[1] + moduleName[4] + moduleName[7] + moduleName[6] + moduleName[5];

            var symmetry13 = moduleName[2].ToString() + moduleName[1] + moduleName[0] + moduleName[3] + moduleName[6] + moduleName[5] + moduleName[4] + moduleName[7];

            return moduleName == symmetryHorizontal || moduleName == symmetryVertical || moduleName == symmetry02 || moduleName == symmetry13;
        }

        public List<Module> GetModules(string moduleName)
        {
            return moduleLibrary.TryGetValue(moduleName, out var result) ? result : null;
        }
    }
}