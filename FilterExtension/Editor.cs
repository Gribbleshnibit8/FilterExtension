﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    using ConfigNodes;

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        List<Sorting> sorters = new List<Sorting>();
        public static Editor instance;
        void Start()
        {
            instance = this;
            StartCoroutine(editorInit());   
        }

        IEnumerator editorInit()
        {
            while (PartCategorizer.Instance == null)
                yield return null;

            // stock filters
            // If I edit them later everything breaks
            // custom categories can't be created at this point
            // The event which most mods will be hooking into fires after this, so they still get their subCategories even though I clear the category
            foreach (PartCategorizer.Category C in PartCategorizer.Instance.filters)
            {
                customCategory cat = Core.Instance.Categories.FirstOrDefault(c => c.categoryName == C.button.categoryName);
                if (cat != null && cat.hasSubCategories() && cat.stockCategory)
                {
                    C.subcategories.Clear();
                    cat.initialise();
                }
            }

            // custom categories
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return null;
            // frames after the flag is set to wait before initialising. Minimum of two for things to work consistently
            for (int i = 0; i < 4; i++)
                yield return null;
            // run everything
            Core.Instance.editor();
            
            for (int i = 0; i < EditorPartList.Instance.sortingGroup.sortingButtons.Count; i++)
            {
                UIStateToggleBtn but = EditorPartList.Instance.sortingGroup.sortingButtons[i];

                if (but.spriteText.text == Core.Instance.config.GetValue("sortDefault", ""))
                {
                    but.OnInput(new POINTER_INFO());
                    if (Core.Instance.config.GetValue("sortOrder","ASC") == "DESC")
                        but.OnInput(new POINTER_INFO());
                }
                //Sorting sorter = new Sorting(but, "Mass"); // replace as appropriate once I find how sorting is done
                //sorters.Add(sorter);

                //Core.Log(but.spriteText.text); // Name, Mass, Cost, Size
                //Core.Log(but.StateName); // ASC, DESC
            }

            Core.Instance.config["sortDefault"] = "";
            Core.Instance.config["sortOrder"] = "ASC";
            Core.Instance.config["categoryDefault"] = "Filter by Function";
            Core.Instance.config["subCategoryDefault"] = "Pods";

            Core.Instance.config.save();
        }
    }
}
