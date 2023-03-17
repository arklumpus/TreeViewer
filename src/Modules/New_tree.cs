/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Text.Json;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using VectSharp.SVG;
using System.Runtime.InteropServices;
using MathNet.Numerics.Distributions;

namespace a36a54db04e2447868b84ad4e188c3285
{
    /// <summary>
    /// This module is used to create a new tree. This can either be a random tree, or a tree created using the neighbour-joining/UPGMA methods.
    /// </summary>
    /// <description>
    /// ## Further information
    /// 
    /// ### Random trees
    /// 
    /// A random tree can be created using one of four models: the proportional-to-distinguished-arrangements (PDA) model, the Yule-Harding-Kingman (YHK) model
    /// (also known as the Yule or pure-birth model), the coalescent model and the birth-death process model.
    /// 
    /// The PDA and YHK models produce tree topologies without branch lengths; branch lengths can be associated to these by drawing them from a probability
    /// distribution (available distributions are the uniform, gamma and exponential distributions). The coalescent model and the birth-death model determine
    /// branch lengths as part of the tree sampling. The PDA and YHK model can be used to produce rooted or unrooted topologies, while the coalescent and
    /// birth-death process produce rooted clock-like trees.
    /// 
    /// The trees generated can either be labelled or unlabelled. Is is possible to specify the number of tips in the tree either directly, or by providing a
    /// list of tip labels. When the option to keep extinct lineages in the birth-death process is selected, these are left unlabelled.
    /// 
    /// Labelled trees can be constrained to follow a fixed topology, by using the tree that is currently open in TreeViewer as a constraint. When this option is
    /// enabled, the topology induced by the leaves that are present in both the constraint tree and the new tree (identified by their labels) is forced to be
    /// compatible with the constraint tree. The constraint tree can be multifurcating; when this happens, all topologies compatible with the multifurcation are
    /// allowed (e.g., both `(((A,B),C),D);` and `(((A,C),B),D);` are compatible with `((A,B,C),D);`). The constraint tree can be rooted or unrooted. When a constrained
    /// tree is created, the constraint is highlighted in the resulting plot.
    /// 
    /// Under the PDA model, all labelled tree topologies have the same probability of being generated; this is not true under the other models. Note that applying a
    /// constraint will change the tree probabilities for all models except the PDA model (this is because the constraint is applied at each step during the tree growth).
    /// This means that if tree $A$ has probability $\mathbb{P}_A$ in the unconstrained model and probability $\mathbb{P}_A^\star$ in the constrained model and, and tree
    /// $B$ has probabilities $\mathbb{P}_B$ and $\mathbb{P}_B^\star$, then it is not true that $\frac{\mathbb{P}_A}{\mathbb{P}_B} = \frac{\mathbb{P}_A^\star}{\mathbb{P}_B^\star}$.
    /// 
    /// When creating a tree using the birth-death process, if the death rate is too high compared with the birth rate, the simulation may get "stuck", as new
    /// lineages keep being added and removed from the tree. If this happens, the `Cancel` button can be used to stop the simulation.
    /// 
    /// ### Neighbor-joining/UPGMA trees
    /// 
    /// The neighbor-joining and UPGMA methods both use a distance matrix to estimate a phylogenetic tree. The distance matrix can either be loaded from a file
    /// in PHYLIP format, or it can be computed by TreeViewer starting from a sequence alignment.
    /// 
    /// If a sequence alignment is provided, an evolutionary model must be chosen to determine the distance between each pair of sequences. The options are:
    /// 
    /// * The Hamming distance, in which the distance between two sequences is directly proportional to the number of differences between them, ignoring multiple
    ///   substitutions at the same site.
    /// * The Jukes-Cantor model, in which multiple substitutions are accounted for, and all changes are equally probable (both for proteins and for DNA).
    /// * For DNA, the Kimura 1980 model, in which transitions and transversions happen with different rates, but all nucleotides have the same equilibrium frequency.
    /// * For proteins, the Kimura 1983 model, which estimates PAM distances based on the percentage of differing amino acids between the sequences.
    /// * For DNA, the GTR model, in which every state change has a different rate and nucleotides have different equilibrium frequencies (note that this model is much
    ///   slower then the others).
    /// * For proteins, the Scoredist method applied to the BLOSUM62 scoring matrix (in which amino acid substitutions are given a certain score based on their frequency).
    /// 
    /// In this case, bootstrapping can be used to estimate support values for the branches of the tree.
    /// 
    /// In principle, the neighbor-joining method can produce branches with negative lengths; an option is provided to circumvent this.
    /// 
    /// With both methods, it is possible to constrain the resulting tree to follow a fixed topology; this works similarly to constraints applied to random trees. Note that
    /// applying a constraint will significantly slow down the tree estimation.
    /// </description>

    public static class MyModule
    {
        public const string Name = "New tree";
        public const string HelpText = "Creates a new random tree or builds a tree from a sequence alignment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "36a54db0-4e24-4786-8b84-ad4e188c3285";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "New";
        public static string ParentMenu { get; } = "File";
        public static string GroupName { get; } = Modules.FileMenuFirstAreaId;
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control), (Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = -1;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>() { ("Neighbor-joining/UPGMA tree", null), ("Random tree", null) };

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABfSURBVDhP7ZJRDoAwDEI7T+b9L6Wj0oQGTRa/937KCuyrUVxrnIw7cCmNrM5yTa47cCiN8lCGxkxDwZbSUA9lfQ/ODI0Jnw0tFJZ9C32h2YPzN/uD/QFop0y5xHPKETeXgm8zg66VngAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVEhL7ZZRDoAgDEPBk3n/SyklJZIJ6xLj397P6iztD4nWMnE1KEPUBmWMaMHwYTbOvowAN6XL8CEcGrO/UMBJ6TL7EI5nTK72wEXpYn0Ixw6TqzVwULqsfAiX56WBeD777nVNI1fPhli2GepgBJtxcP5GFkiyQJIFkiyQ/F7w6bdlx/M9KOUGPM55uMafn1wAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACkSURBVFhH7ZfbDoMgEAWhX2a/vP0zuyeZmlShUS5pH868sLvCZiCRaE471oCwl3vO+Ulc5cY4g0fsZSE+j05AkJ6GZWJhfHNNgkXNAsTtEizoEhARtkkwuVtARHpdgolDBESUPiQo12HeMAER5U2C0kb1Hoh3+PDsG6XmJfZ9Z94DbWgngnQYtD30/fkJWMACFrCABSxgAQtYYObfcZE/+ypO6QW1oujaruQHcQAAAABJRU5ErkJggg==";

        public static VectSharp.Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            VectSharp.RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            VectSharp.Page pag = new VectSharp.Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static Avalonia.Controls.Control GetFileMenuPage()
        {
            return new NewPage();
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true, true, true };
        }

        public static Task PerformAction(int index, MainWindow window)
        {
            window.RibbonBar.SelectedIndex = 0;
            NewPage pag = window.GetFilePage<NewPage>(out int ind);
            window.RibbonFilePage.SelectedIndex = ind;
            ((RibbonFilePageContentTabbedWithButtons)pag.PageContent).SelectedIndex = Math.Max(0, index);

            return Task.CompletedTask;
        }
    }

    class NewPage : RibbonFilePageContentTemplate
    {
        private static string Icon32UniformPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACgSURBVFhHY2QAgqKiIgcgtR+IHfv6+g6AxOgFGJEshwG6OoIJiJEtBwF0Pn0AMCT+Q5l0BaAQGFAw6oBRB4w6YMAdAK4LQABUEAGLYDgfHXjVbiSroNrW7I/TTBAY8BCAg9GimBAgJw0Qo2c0DYymgdE0MJoGRtPAgIQAyLNADOqRDWga2A9yBEqDBMqkK8DbWqEVQPKs40DmAmAvvO8AAF4OQOcTBSHuAAAAAElFTkSuQmCC";
        private static string Icon48UniformPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADeSURBVGhD7ZpNDsIgFISpJ7NbN96FvSv33MWN23oz5SGY2Ihp/Js3cb6EtLRJy4TpBChDqMQY1/kwXWthTCmd6rlrioBZ4xsUIoZO4xvuRaxy6TXeeHbPH7k3zlZqlQLrAWokAI0EoJEANBKARgLQ0Au4TWiMNpDLQ+i76z02u8NXBn7H/XbR+w1ZyBWaDwD4bwGWQp9MoleeJwu5QikEQCmkFHoTpRAaWQiNUgiNUgiNLPRrqkvsz2qBNYWmJoLZQkXEw5U5Jug/4sVrkF6YuWRk7oGyj4NVQN2EEsIFUgxqZ7TutCcAAAAASUVORK5CYII=";
        private static string Icon64UniformPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEiSURBVHhe7ZtBDoIwFESLJ5OtG+/SvSv3vYsbt3gzpTi/QZFAsCFpZ15CRgIJ9uULP5U2boT3/thH995zbQjhgc/VkgR8Dd6oXsIgYGbwRtUSmoXBG9VKOPTb0uAja86pg74innHDbvXECqBGApC0SACSFglA0iIBSFokAEmLBCBpkQAkLRKApIVewMf/AhGbDgshTI6t4XS57Tqddr+eN31PQz8BJC30AiZoWpwMCUDSkl1A7AP26AVyXUcVgKSFXsAE9QFkSACSluwC1AcUhgQghaE+gAwJQNKSXYD6gMKQAKQw1AeQQSUA1R2XCCUYK6AbS8guoJA+IElgvgcMEmbfEGGB/inw1/s1pfGjulvmChhWw7IKSEuBGQWM1kE79wJNIHSGSHXjRgAAAABJRU5ErkJggg==";

        private static string Icon32GammaPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAA+YAAAPmARHyHpUAAANkSURBVFhHvZdLaBNBGMdndlNLE9qDHi2IoAhVKUXQi4fEg7Z5Nq3VSy0KxYOgxdQHapOwSRtBZA+Fgo+DFUQ0hdYm27T1kmgR0SLiQauefBxaBRG0kD52M36bnYRskqZJs+YHw3zzn93MPzPfzO5iBLhcLiNUUSgmnudjhBBZrghMxuAyUdquGAyU1OApstuVAf55et7lJahUkWcgTUpM0eIeN1vdoTtWT/gMx3Gqa7VizR+1uEM9DMYCwcgOtoZeS033MEbJpNWSvAaaudAOGOoGhCOGj8tbCcLXIO6y9IUvJS/QkLwGGBH1QbUkstLZYLBDivhsAYTwQ4JJwOwWDitXaUOOgWOukRqYaicm6PG01/mTysiwuNQNCfIB48Rw29WxLVQumxwD8dpNzVDVJRB+pCgKQb4jzmC2E8LNKzr2lqKWT44BQvAhyLTFuK72OZXShH3Wd7BHPFCOmj3jXVQuizwG0EEY4GXUaxSppOIA+/YmJGUMNsSgpS+yjcobRmXAzE3WQfbvRQS9oFIOXq83QVjpFISYMNJwueeD6mZmdWUPVGwCkVlFyc+kt/ULJOo5jIjxldh0nsobQmUgweLdcs2QqvdJoQACZ78P1SgYGTB7Qk2KWjrq6SOoQU7AyID5G1UKglfZbrhpAX4k6LgcqqVySagMwJQ2QALOyY8EKhVEuG75DYZPwMXbxWoySOWSUM8AwjvhAPpMG0Uh+BwzYNcP9560ekLyOVESWQZQPeT2VxoXjeHTcj9Uz2Ambls9T/YpanFkG6gCqWQD8vOiWpTaIZyHJ3zYzI3WKz3rk20AZjJRsgGZ0YDzFyaSDUI9EnXjRy4+NSg9hckxkGDQdxqWjOB3ziGCO2FrNupq4g9MXExHu9Ykx0AcrRS1Bddiwm8T4Dzvgal06MW/65pQGZDPgKi3Y5E2N8yE3zEE1QVI6ON66c9YoeVQGYAsTj//y2XCZ+fhde40hM26mqWZFndol9KjJssA+UFDTYhw9rsYEyuE9QxGb+A984qdE/RKr0LWEmDNZiCFwDmmJUlshHAaBgisipLqw0dtgGi3BJlMDbTNw5K0w1vW/qn+1khvby+BYsSwXVQGoLVAo//CpM82m/HdkfwMTL/nZ34ZVZK0gUqS8WdN6iWoLCae52P/ADsRdExVH9UVAAAAAElFTkSuQmCC";
        private static string Icon48GammaPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAAUjSURBVGhD1ZpraBxVGIbPmdntJv4wUbBeCgoKmlYQiiiIRRNvzd4mREzwAl6opKJYS6I/tNkdNhtDpRi0KDX+sQWxmmjT3Wy21YLpD6sUChXRqsVbRWvWtjReYk12Z47vd3a2NmFms012J9kHvjnvOSdk553vXGZmlzOLzs7ORhRj+Rpr6u/v329piRDCUksLhQ6zTp4Ys9qWPIrNyReoChOUAbuTL1Csb0lwdg4QuOJyoGP8z2gnlvQcqGZsDdDVnh1EMDZ8bSg68mwwmnxibWx4uWxcZErOQCiS2MQM9QjsbEF1m8dQv/XrqTvzvYtHSQZCkZGnBee9kCrie8Q4ol4RZsofHbkJetGY00AgsvsawcVLpDGQ3s1kxld6VHYjqt8gahQmhps37bqc+heDEjKgRHCoRfxknKl5/NBARzaha8eZarSgbQKxQlHVDwIb9vigXaeogdbY7nrO2f2kOYbQh1vumZQdYFRvPYqUPABpcMZv4RdlX873uEtRA9Mmb0Xhw6bwt6LwnfnW/xmNa3thgjJE4+vJYCTZLLWLFB9Cgj+YL1kiqYf+kXoWN3sO0/ygHRuJYDvcXl4dDTTHhi5GIe+FTEW8R6Uduq6bzPQ8BvknYrnXUF+XHS7haEAxl92KwoOYOqPU7ZONDoz2Bo5hrmwgjZXqvkA08bDscAFHA1zwNZY8NKY3/mtpR1IxbQeKQdIYS1uD3emrSFca5znAmTSAPeATWS8BVVWeQvEboo6rubfb24do46sotgbkmi7kZkUcsMo5wUQ/KYTyKCTdPq2ZvM7Xme+pHPYZuDC3Gke5MdVkzU+pLJV0PPQRigFZ4SzujyVvkLpC2BpQVHa9JX/d1dd6ytIlg6HUheIowqcYbGdTbH+N7KgADnNArLTEEas8L/J7hqCVKIdYdYH5V5/sqAC2BjB+V0nB52eAGO1pOYh5RHew9A83hvRkUOoy45QBaQBL6bwNENil4yg+RnBclLdaYskrZEcZcTDAr6SjYS7MAO3SQs09AnkScUnOYO+Ue2l1MIDBA2rN7NeytgDS+r2/cMHWQdJz6e2TDb7nZUeZcDJATMxnBbIjFdeS2BBfs6q6vztV2OUXTDEDx6yyPJxe9hyOhxEeRTHfD3enVsj2BVLEAP/ZEmUhvdU/ZaisHfI04lJDMYbK8RTnaIAzUd4MgL269p3g/CFIUz7F1WUXfOvtaADLXlkzUCAdC+/BdO6WFc7WBaIjL0g9T5wNKKIiBoh0r7YZn7CdNDLdG4wm8k9+88B5DhhK2YdQAWRXZDKZDkh6UMKSzbeHIklNdp4njga83splgKDXM9PTU20YTp+j6hWcDYb0xNp8b+k4GTB8X01lLF0x9m1u+0P1KHcjJV+i6hOCDwf0RDjfWxpOBk4NDrYZlq4o9BDkMfldkPSmrxb3X8PBSGK97CwBJwMVv/rnknhRy4ic9zbIgwiVcf5GUE++Uso+YWsAK8PvlnSNdJ//BB6E7sCnj8gGwZ7h9dnPtFiqQdYdsM+A4PT22XXoQSgdD7fg5DeimkWsNgzzi0A0ORCMpS+TfzQLWwPYJk9Y0nVoiR2Na6/i1rUJ1R8QXqyzHczM2b4gcBpCrs4BO9I92oFMZrwBtxzrYea4avJtnHPW1dUlEI2kKWwNCMZdnwN20F6R6gm/ySa8Vyfj4R+tZuLsV8C2BhTu7io0F3Qna8lzkSZmfJ1a+Jq1mrBfhaqIGRmoBmaNkqZqzoD8RU21GrB+DsTYf9RQu9Zl2h8nAAAAAElFTkSuQmCC";
        private static string Icon64GammaPNGBase64 = "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";

        private static string Icon32ExponentialPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAA+YAAAPmARHyHpUAAAI7SURBVFhHxZfNaxNBGMbnnRVyU/EiWDxKPXjQi1DwYAslkiZbPPTkxUtv2kMCKmoapzcpC/4FgtSbBzHdtJhLai9SRC+KVUERvIuyolh3Z3xmZ1PcfFkls/nBm/cj2Zkn88UOKaVYpVI5yxhrwSY9z9uAzwwnCIJ255qLzWbzST6f/5jk1uGwdudtOnO76CnQVi6X4UycpekR2GVmsf6idNMfS9JMSAkAp0KHHU/iTOgUwLiMRirgi+JsPIkzIS2A6B2pUQqQ8i1UjHIKaBsfRwtifb/J7ZMSQEw9046HO6dNxT4pAc4ObcFFktGEqdgnJeDRbTeAe03ERiNAoxh7CjcBEWQqdukSgH61gIOlW34m27FbgMM3tY9CmY8LlukS4NdmPsC9xFCcNxW7dAnQKEYP4c7MivoRU7FHTwHc4ffgKAzZvKnYo6eAZBoeYxrmCwvrOVO1Q08BGmzDZbgxduDXJVOxQ18BvnBb2JJrRHR99kb9cFIeOn0FaLhDFSzJHN6SVoQQA3/7vwxstF4rvsHJuIBwekuevGPjdPzrv1pbcu+i22VSdLlQXb1/Tjw4lHw1FPY0rA3hXoGAa5iOOSfKbRerq1eHtS7iq5kG1zOFa9nAIS6KxgkVhR4em0YawZ5jdDbRxivU3u9z+KeA/fjcqs19ix/og+4LLr4G/pOANqWqf0xyeQFH5hRS/fLSeVb8hH3XAVqXaPSrjkHYWHLHEwGayZSAOMiYXQFZ8ucIWNnbewRrwNv4DZb6DRFWsnrvAAAAAElFTkSuQmCC";
        private static string Icon48ExponentialPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAANJSURBVGhD1ZpPSFRBHMdn3lvTCKIg7H/dKruEKB0qSg+xsSubVnjqj10MPAXbwYOtvZaKiKVLBEV0jgRRd7OSSI0uZVIQVBBWl0o0hFoFy/dm+v1mZ8PkvXU1c3/vs8z7fWfe7jrfnfm9fyOXUjIkGo1WQehVFcaqE4lEn9akMXAzo/NIr24jj5lOp2d2PktDT09PfzAY/KTrJMERcOt8llz7SPAnBxCYNqoC85+rBh+gcsDPuBoIxZK9ESu1SVdJ42qAM1nl2GK7rpLGcwpJzrdpSRpPAzAKW7UkTa4k9r0Bf08hYG2d1bFCa7LkMsDsKbZFS7J4GRjBjTBN8nngboCzdxkhyzKRLl4j8FptJStXkTAeBuSgFpU6ksXVgDB41sCqkNW+QWuSuBoY/TL8FsIkai6KKjBSxdXAixuNU5LJl6oi5S4VieKVxHgkeqLVPh1J4m2AGf1aVISs+8u1JoengaJJ+RSCDSXAhL1bNRLE00Dn5Uga8mAAtSFlSDUSJMcUwo4bnRjhTr+Oc8gKguQ0AH1u13L9gbNJkie1nAaS8Zr3EN6gNpg8gpEaOQ0gUrI7WjbUW21LtCbDrAaEsG9BmIJSOiFKDqtGQsxq4MGFQ18hdKCGG/0mjJSY1QBiSHEdI0ynPeHWrv2qkQh5GUjGa3Gt4JGqSHbFsqy8PrcY5N0RzkQzBHz4u2NAlB9TjQTI20DqfO0gdF8dkWAqXQ23dG9WOwrMnKaCHXBOQ8CkXslM527lqZtFakcBmZOBh611I1zK4yAFDMPO0tVrrhU6H+b8x1Pxg5jMF1HDxVHjM7v8dn19m4n1QjCvX687HonBxV0CNcQTE2XFjyNWqiCPIudlAJJYpqzIGRgCC6vw2us44lU4lrwUbOncmHnX4vBP8/eeFTknuIEnNrzoKwYvzQGDfwzHupI1sa6j/8MMruNNXwJekEW+aquvZJn9PSo4b4IPrtPNWT7Alz6HuTYE55Ih5kDkzqgpzLHP34bH8AGCfl9eZPsIqMX4BV2lBCOBpfZ4DePiJHwB/kr53Ev/gOJA+QVlAhsUko2D6Yw5Ln/CaKvb2mkGkGpXA36ioMfwheCvEfADM6eQn0dAJbFfDeh/B2LsNwmKFUThrIa1AAAAAElFTkSuQmCC";
        private static string Icon64ExponentialPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAB84AAAfOAVQWu6sAAASHSURBVHhe5ZtbaFxFGMdnzm7salMRSiv0QbyhEgVBC1K8NEE0JtlsWsFI8IJQRX0rzUOrNC7bBH3Qlb4EoeDlxZdGzGbTpCQvMRGkFRWfJKEWFPHFXhSTjS27Z8b/d/Y7203N2VuS2pnzC8v8vznLLvPfb76Zc06O1FoLn/7+/lY0M8VItKXT6a9YW4vD7dWDJ2a4z2o8A1YZvI/1JkQWFxeDBu/zyvT09Gx7e/svHFsFZUClwfvU8h4jWVEECaS814ECKL0OyykVwbASegMCp8BC8+40Dt2tImr/yeQeKwsgEZgBGHw/mh6nEHmg2GMnVaeAFvpellZS1QC8IdwGaCnuY2klVQ0A4TQAu6AlltuefXt0K2vrCDQAa+EZliIfbbqHpXVUMmCBpVDStXYaBE8BecUAmGHtShBsgBbzLKFl+DJAlU0BEL4MiEYcMgA+eNy18/VjTaytItCAbDK+jMY/CWq6dcf2+1lbRaABBPYCP7AUUkUeZmkVFQ3AGWHJAK3VQyytorIBwvmeJZDhy4CYmy8zQDxoYyGsaMCX7+69gDz4lcOYjYWwogEe2jnFSshC5HGW1lDVAGyJ51gKLdVultZQ1YCCELMsgXwChlh1v6CqAVOD3T+h+aMYiW3tA+MtrK2gqgHYC2j85KVpENHKqpul1Yugh7xyb1DKOCsrqMkAqeQYGv8OypN7U5lbWBtPTQaMD8V/R+nzN0VNedd5hrXx1DgFKAt0hiXqgtzD0nhqNkBFxShLuKE7ew+M3MiR0dRswGSyh5ZD/zLZluUtm3pZG03NBhDYA33MkpbHN1kaTV0GiLxDBtCVIuKRrsPZnayNpS4DTrzX9Sd++REOKSXeYGUs9WUAkFJ/xJIMeCGROnEbR0ZStwETR3pOY+DfcBhzXTXI2kjqNoDQrqT/HvF3hi8iC4y9XtiQAZND3acwftoeEw6y4APWxtGQAYTS8hCafDESbZ3JsedZG0XDBpwcTCygFhzjEAkhh3tS2R0cGUPDBhCF5dhBKcTPpNFuzbv6i97UyA3eQUNYkwFT7z+d00rsgyxQjJ3irpwbGzbpstmaDCAmhhJz2BxRPWD0qx0D2Q9NMWHNBhCTg4k0asCnHNLI93cezn5uwhnjuhhAbF649Bqa8m1yX65503fxdzLX9S21dTPg+PHn3M3zl/sw8GHuIlq0cL7tGhj/pCOVuZ37ris25HmBeHJ8H86Xj+KDmrmLcPGBozDos5xz89RMstUrnP83G/bARDw1cad21VEUxW7uKkNeRP/XMGkWJ1dzN83nf6QM4oMbBo9txdNwG/7ESPdAplVJ+Ra+6imEq34mOpfwpWfQntVCYl+hzmIV+Q0GnXd05GIhGr0wmez4m9/eMP7YQMmEa/bITGdqrEUo8bLUsg9hI6fQNGUW8foLL42/JZybYyuuLyH+B68VYBA05c5NHEm8xF3lBhCeCdfMgHJoeii38Bj8fxRfQplxR/HIupODAaU6dJUBRFugAWFh3ZZBU/lPBtjMalMgzBngFcGwGlBaBsNoQNlGSIh/AbmDiNqBRgPoAAAAAElFTkSuQmCC";

        private static string Icon32NJPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAPnAAAD5wHDtfxxAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAfBJREFUWIXtlb1LI1EUxc/cyTMIVoJftcVaxMrCxmXRaLF+/RP2A+ExtSzbZRgC+U/W1ShiBNFiGytTqLWgRNhKdhlfcmcLZ8KOJnHeOGEsPOVj7j2/N/e98wzf95GlKFP3D4C0AaSUp1LKU52aXJoAABZ0CzIfQS74ZcNEtOo4TnNQRrZtjzNzDcBf13U/h+sEIA9gjpnrtm2PD9C8DmAu8OuIlFJrAC4AFJj5pFQqTaVpblnWGDMfASgAuDRNczMCUK1W75VSxQBihoiO04KwLGtMCFEHMBuYL5bL5bsIAAAMAiKOeQcgbYi45hGAEIKIlgE0AojDJAdTCHEcmDeI6EsvcwAwur2G4Q4ezNHZ2/ynVhvi3ifaqn3b2O/VaH17Z81kb9eAgQnvCiPt33133hcAeLo61/n5mxYNiX4Nukn4Hqb//Irz6Vn2SdhtMRjB0eTjlbjLz7TaRq7p+8bW3vfNg16NVrd/fh1irwb4mPSuAeCSmZcqlcptP4AXI/gvtQp4OkTFuBEtpQybNeLWR0bQJbVWkrwPSqklxEzXDoDO3X1NOplCaZvrQtAgzHUgSAixh5iplRTiWbr+iAAA8ACc65x2XTmO0ySiIoDzwK+jnkmYROE1dF3XiFvzPpPwDTrTLUh1BEmU+Qg+AP4BBeQXV/O+iUcAAAAASUVORK5CYII=";
        private static string Icon48NJPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAACXBIWXMAAAXbAAAF2wGkowvQAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAA6FJREFUaIHtmDFsFEcUhv/3zjICscvJQJqIAsn0kUWUSCkoABOaJBKKG6AAgRSFNLvrimazStJ5dxuIogBGCpAiLlCUImAwbZREgig14lAqEoRFbiM4H755KdiFYX0H41uze5b8VXPv3s38/9682ZkhEcFqhqsWUJQ1A1WzZqBqhsoe0HXdUSKaBQARGY+i6HaR/kr/B5h5AsB2ANvTdrH+iktaHiIy3K3dL6u/BlzXHQXwIYAfi87H1wERkeu6BwEgiqJLkts6MBFdJaIpIvrVcZyxamR2JwgCdl33LIALAC54nncon6NPoRFmvjYoJoIg4GazeQbA0ZflsVLqYwDz6eeBMNFD/LRlWZfyuRzH8U2l1G4iepDGRph5bnJy8u1S1OYgIkqS5DQ08UR03rbt477vq3w+A0Acx390Op09mom6iMyWbYKIyPO8r0XkEy123rKsY93EA1oNVG2iH/FA7j1QlYl+xQNdXmRlmygiHujxJi7LRFHxwEu2EqmJcTxfYusicmUll1gR+UwXj6dLpbF4AKBXHeodx3mrVqtdF5HNLbZwb92OxQXe+ESIbnFHOT99+dFvpoNNBDPDtJDcaNXs9wDAXryHre27IMiyn7yxgdTEmKqtn2ts2FnvvHiEeExCEYDEZDBFsp+AXXps5MlfeKPduA8gBmAsXkQUgMtGBgDgwMnvvmoN1U+aDmDKkLQx+uiXfn9+x3g7TUSv6QKpWLdGR0rHccberK0/0diwE7kp1FIi54jxn9lwtIsE7+qRTYv/AMC/RPRtOi2MEJFHAL5/pYG0iGdZFurbHv+Jv9ftWGyx1RbglgI5P3/xwe+mg04EM8PUTm60+HkRb2nfBYBNALbYtr2yRew4zhgzXwMwkobmlVJ74zi+uZxBdDzP+xyAn/Wn9Q0A0702bb3oWQPZk9cGeEhE7xcRn4eIThHRN1roaJIkZ4MgMK7Nron62p+GHhLR+NTUlPF0MUFEJAzDT3UTInJkOSaWJJUlPqOoiRcSyhafUcTEsy+rEp/RrwkGqhef0Y8JTpfKOU38vFJqd9niM0RELMs6AWBaix1pNptnuplgZp7BCq7zK4Hv+8q27ePQTODpEnswn6s7GgjxGT1MLIFFZJ+ITIrIO4MiPsP3fRVF0TEAhwEcDsPwYj5nKL0PDUtXZ0h6F3oRAMJwqcxVfztdugEiandr90vpBpRSPwBoAGik7UIYHykHlbUaqJo1A1Wz6g38DztHQCT4G9hSAAAAAElFTkSuQmCC";
        private static string Icon64NJPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAAfPAAAHzwGGUVlPAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAA5pJREFUeJztmb1rFEEYxn+zt4nGIGgkKRTBQhRstPB/EEGU+BXFgHZJo8JNCgs/Ci13AqKFVlrETwwSET8KS7GxsBIsJOJHCjVaqNHk9jIWuU3i3u3l7nb2ZhfvgYO7ZWf3eX63NzPve0Jrzf8sx7YB22oBsG3AtloAbBuwrRYA2wZsy7VtIEpSyl3A1dLHAaXUwyTuk+Yn4AqwtvS6ktRN0gxgXcR7o0ozgKaoBcC2AdtyQ7PtCaXUqE1DppTP53uFEJdLHyNXEYd/Z9vbUsq+JnlMTFLKPiHEXWpYRcI/ARcYyTKEkvcRatzjOMBJwF90LLMQIsL7WuvjUWMcpdSoEOIw5RBuSCn7k7FqXlLKA5SHLwohjg0PD9+PGucAeJ53rwKEHHA9CxBK4W9SHv6o53k3qo2dnwOyCiFOeAhNglmDEDc8VNgIZQWCifAQsRNMOwRT4aHKVjitEEyGhyVqgbRBMB0eaiiG0gIhifBQYzXoed49oJ9yCNeauGMMh/eBI3HCA4h6/hscGhrar7W+FRgpiGV8bt+of7hrpkEUQD+b1eLU4/O738QxteP02PrVs1/f/3K7AOgsfqd7Zpz22anglNjffKC6AMAChIJY5r7r2E5RlNUc3/1ZvfXphT0fGjG04/TYetdxXoHuWnw8h8+GqZe06Wlj4aEBADBXdHxs33LrZ1u3MGGiVq30v7Bu+rWpy00AAw0BANh1dmxKIzpMualFjvbZNPXc5CU/ZaolJoT5B64hAFLKvk5/crlpM0tphT9p8nITwGDd/wwFdXdPYVz8dldRDDVeNEyS87c9Orf3YyOudp55sNkRvABWLz7u6AI9M28BisAxpdRII9cPq645ILwZCZbBn7muP1o4BeCJzvmy0fCB5pfB3ByDzuI3embe0qZnglOMQagZQFS7CehXSt2Ja6TC/cLG/CTuXdMcUK3dlET4SkqqbbckgKT24PUqqZqkKoC0hA+UBIRIAGkLH8g0hIoA0ho+kEkIZQDSHj6QKQj/AMhK+EAmIMwDyFr4QHEhOJDd8IHiQHCklPtIqN3UTFVr2+Xz+d6ocQ5wkSZtb5NWyXMYgiuEuBQ1JrwKZDZ8oAgIkXKAQeZq4wngUJbDB1JK3dFaH2Qh12DUuQ23xJJWuBpUSiXSf8xUSywJtQDYNmBbaQbwKeK9UaUZwOLVKXIWj6vUrgLNUpqfgKaoBcC2AdtqAbBtwLZaAGwbsK2/ahroJEitsccAAAAASUVORK5CYII=";

        private static string Icon32RandomTreePNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGSSURBVFhHzZbNTcQwEIW9iBJogAYoYE/kChLUQAu57YH/A7e0QA0gwRVO3KEAKAB6CH7szGpkxmMT2yifFNlOvHnjN2NnF64hfd9f+OZ8PdLZorYVpnhzvAMjLhqqTE5Bjr3MMAxRnZIUzN/e1in4ebFlb0octN4Fl9TWJ8feHMwUeIFkpVspyCGVgvlW+n+lIFrp1jPGz5n/t2Cb2qZYLhWnQKHz85+pn6TEARwy0uKN8MHp3b5vntAnusfrYzWohbGSDZaFEil8c7J0e7s77u3jy61uX3AL/AqkSgCacEgskMk1AFLCh2f37uHqiEZrwkCo1UEAlkM+gBG8vn+O6ONCX0Obg36VbYiVY6VydVoNaC5VOYhgNeBAIARR3EeLMe6zOM8HVQII8wwhiAJt1XJ+Vgq0OrB2hiy+UDwk5cCkfzShI4y0vgqoYq5mjdh9Bs+LawBVHluxBX4HSgPouNr5hSC0Wo4xD2Paml3WGZ/CW6meiBBid+R54Mk7iv9KGAifAZowUzUARgZCRD7Hzn0DgRxXbhVv1tEAAAAASUVORK5CYII=";
        private static string Icon48RandomTreePNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAAVNSURBVGhD7Zd9TJVVHMe/53nu3RUQEBFz6MUZkTlfCP2jkRiQRhODXLqKNcdsLTeXrbFYa4u82mxrK6q5mW1lmajlmqm8iE5xZAsSExAV0MWrCA0BAeXFy3NO5zw8F+/l5fbc6+71Ovkw7nN+55579vud38v5PQQ+REZGhoU/tg5L+pC0p6/gkvICXzPAZXwthJg2RHZ2ti7dJj0wHu4k42getAfuS3lXmAyh8XAnGQXu/M5nDdDLQx9CvmbANu2pG58KIXd4tKoQP1mXLyhf84DXLii9PHIhpCs5vZnEPmWAOznmayHkco5N5sB43EcIufy7SQ+Mh/06d3nYPCCauERN6URN1oWnDNCjwIjSQUFB22umxhuTs468e3nK8lo+J8qpLoi77tbrYme89FFulCKxdAPBRoWycDHHN1X45wkQ9iPtMh47vjN5UF08AV43YNUHh4KNJtOrMshGylgsIYQti5qJF2LMxBwWiOKqFpy60IiO3kHIROpWQHMoI3sLP0kt07ZwwCsGWCwWqdS67Fki0Q0ywQbK4BceGqAkLJkjJy2NQFiwn7ZyGMYYrjR1oaiyGWcqm+mglUoSIdco2AEohh8Kdqxp1JZ6pgrZSLHkRtAhlkZkbKaURfiZDEr84tny89FmLJw7XVvlnL7BIZRUt+J0xXV6sb5dYowwQlBEGf3JIMu/eswAkZD8fL4WR7TAHMrWxs4jz8yfBaNh/Lpx9tINHPr9GnZujtdmxtLScRuF5xtx8kITvd1v5U4hPZ4ro4SoR2w0yKhu6iAHztTiWGkdOnsH1K9HsyAiBK/FR2mSIyKkyv9px8EzV5F3rgFC+UA/o5gPkrU1uoiNjR0pbyUlJU5LZVR8WgJ/JPzy4WqYw6airasPhX834rc/63C5qVNdMzt0Kgzy8Bn6m4yImBmojm20d/fjeFkjvjhcjtzSelVOWDIbb69ejOAAE6oaOmDQ1noMf5MBK582q/9CgeKLLSg434BsrtS3BZewYlE47HPCFvNFlddRWdfOZwiiH5+BNxLnI25hOEzG4TOvUL+D5w0Q3OzpR01zl6rA+hVPYF1cJFfgJk6XN6uKiriey09flNFzV//FXauiym8mLVRPfHrglJF9yq4O72PDK62EUF4kqA2efIiJDMP765ciJzMJW16OhlWhKKlpgyirX256DrveScQryyNHlBeM3kfgdhVyhqhQyR8ftYBha/72VG12LNYhis7bA3hsmj9yimrxc3Et8rZNvN6e/bwoiMLgFQ9MxGl+UWV+94cmibNx6TxVXDVAd5eoh1U8sT9/a4U65pWSh5Y6dECEze6CKk3yIiKEkrOO8lLNGE9K1tZ1Rx1PxN5T1SzVkqtJ96hu6mS786s06R45RTVM7O+VEHIMlfHhOmkjR54yh2BT8iJNGotXDLAPlYmYKIT+D48b8E1+FeraejBzmmPHKRD9z5ZdxeqY8T+b/vbzo7kzYFVvdL5GdZlLrYQrRCamdUoUQddudM/nF5Xh7KUWZdCqSLNC/MG7UnVNwBQDhCxaiAre69S23MLr8U86zAtEeImLbz8vtV8dqaAl1W2EG1ILRj5zw2musdpSEATl7lpZktJ5S83fd3lrEDkDK6PnkDi71mDPySvI+6seh7PWqLLA1nrkldUr7bf6Zd5+9iqMHWRU2le4I0VNKo8bYI+z94PSmlYUlDVgX+aLTvv/XEtKn7adilcNsCHe0M7RmJWEIh0SWc9DxBTob2R9AwqRJFB+Q0v8uGsoxfdDinX/iU/XtWo/HcMDMcAe2zsyrybvEYZ5lGCPs3dgR4D/ACmsuahYxxwXAAAAAElFTkSuQmCC";
        private static string Icon64RandomTreePNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAB84AAAfOAVQWu6sAAAS/SURBVHhe7ZlraBRXFMfPnd2sSY2hNELbpOmWaikoUq2tQpPQ2GjbxNR8aPSDIA0V/SSIQRQqNZtCC+ZDwK8h9YEtAR9g1GpLW/ERoS1VsaJiKSUbX4jYIqXmtdnpOXfuyuy4r5m52Zlr5weTO3Nn986c//7PuZM7DBSmo6Mjhk2nceQMZQWQETyhiVZFXAdPqCyAFFROAV3scnp6ehzFEjhAtEVFVgEzo4wDpiJ4N3iRAr4JnghqgGiLhqzqHcwCknhiHOCU/70DVBagS7SuUFYATB16nnAtgrI1QBbBLCBaR+Cv6fqxVlkHyAjeD7hJAeWDJ4IaIFrbOK3mwSzgM5R2gIxCrKwDZARPqJwCUmahoAaI1jZe1wBZ4wQOEK1tZDlAFoEDHOKFAFJWcmRRdAHQqm5Wcuh7S8juFRUVIdpo36n9CSYrJ93chB3e3Xa4poRpq3Wmr8fDCLZfJzXW912s9Q/jE/ZQQoAVWwZmJEr1NrzddtChHrvSr6VjCBo7gzt7wqPs4JHu1n/Embz4VoBYLKb9nHi9gTH9Izz8ELfp1F8aCUHtnCpYuqCGDuGHizfg3NXbMDo+yY+Rf3E7pOts7+LwhVM4TtLo5jPQY4/PvhPAYvGXjV6A2VVPQ9MbUXh7XjWUTQuLXoPxiUn45fe7cOLXOFz68x43BBFOjsFEKLLDmiJmIRzftFU4NwKs3LS/7OGMaS144xR0I258rJkVZdDwWjW8tzAKVc9wA+Tl3oMROP3bLfj2fBzu/EVmSMHO46C9oTG935winglAFv9pYuFbTEuuwaloNQ5WTv2RsAaLXn0O3pn/Arz5yrOgac5uUUc1rw7/DScv3eCCjIwnxBkYxe0oOmxf+bXx454J0Pzp4SHUP0r7DL85N1rJ87oO89tq8Uxs7hvEugfQvbZO9GRnZCwBg1gnqF5cid9/lCIoU9zDJ0EjeKcYxnD8+wlY1PEI7h0wwL//PhY2s0WnOgVS4w9euc2PPRfgm89WZK3iMougdRZZvv0I7/eFAGYKDcCMEwF9K0AKsvDlofuPPejkexAyn5/3UiUW2My35TsBclX17FXcoJBZxDp+SgDfrAfkquoU0LIFL8KOj2thd8cyaF86xziBH2+rnw29Gxv5OfpMtik02/iZr1gAVgfYgdySLwVyQenR0nmU2/tY1wei1x6+c0A+yMJbvhzk+6kUyJLetnAjQFFXdswWTllPQvzOBUAbu1nZsQ0Vr+61tXyfUoDIVuEJs2NyIUNER7ipAYnJJLR2HYNwSIOBzhbRm44RPHskmhXf1YBCfzGikBpgdkwulJgGrfiiBsim0F+MI3Ea8FyAr05et6zcZMacIkkRv+Eag0JTiK5F10zhoQB6nP72n7oO63b+CFt3nYPvLw7zx95MpE+DhgLMlAS5UojGpLHpGnQtuqaBHs/8jSLgZkmMAmr7/Dh/7D24rVn0pjPlS2Iysbso+hAFWIkCPIUCHLAIULRF0anCtCy+Dg9nGb3p6wFJVGrVFydgemkJ7P+kKet6AHIz35sj3wmQIteLkcWYIqcv38L9MP7v/3zBL0Yy4VsBzOR9NUZ1kcFZbOy/GhOtMlCKhADWMI1twJuPJJne5/zlKMB/gX+62xYG3yIAAAAASUVORK5CYII=";

        private static string Icon32PDAPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEiSURBVFhH1ZXPDoIwDMbBJ9OrF9+Fu1HjnXfx4hXfDDvoSBlla5cO5Zcs+5Nlbb99jLoypGmaO3S3cSbjgL0VquDmgAK9azgVob4Cicxt24rPzbmC/5V5qysYAnAya4M7rL+CB/ZiogpARauG0xgtRkqB4oYTXYGr1jdcMsPaA2pSHlg4PsfpMeVqyYFBAuofjmkCm+KSy5Fcw89NuI+vQIPWLykF1G/77pjJdb6+OMlP7+flg2PRHg/sPULXjTN+j8SEHR4UY7EnCO5gz2ETgExr33CJHjQQ20ODh3vCJEp9hpPs2Dv8eFZM0XcAKp/unI4pbAIgU+8bLtFKTJEowLpXAi2EFDMjacLc4FJKe4AWwj7R2Qkw0mb5xEqBTJ9U1RfFTaBjG0yPEgAAAABJRU5ErkJggg==";
        private static string Icon48PDAPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAS5SURBVGhD7Zd/aFtVFMfPvUlKB+1EUBRxAysTBXUI4n8iIrrmR5Mm2okiqEP/UP9ambhKQozJtCJ24qAgOhVEkOmWpKRpNnEDFURkQ+YvEHGgooITNyG6Js09nntz8tbXpGlekxpH84GX+73n5b3cc8+P9yKgC4yPjz9DQ7w6aw/J439NRxav6ZYDHaNbKYQsYWpqqq019CJQY7WF+X+KQMcK0wm9FKrhpDA7WcRdd6BdLvgU6pYDCR7bxlEKtdoq281rJziNQFdaZTPWVxtt1mk62Rqd0OtC3WZNUqhdnKTghR+B1e5cgwh07H+ukwh0zIFu0SvibtN7DnSb9VsD7dDJ+ll3KdSxf1KdwhbKkYnsZWUX+l0S5isKKmw2CESUbvepSsl1qvC873c2G3yxbAhQXSxAnLNdRLgQTlfc8EshMfoNm5ri3/3+kOrruw2VMmuT4P40nwp8a042wOaAWQhApjpbHiqEGQSRKiSDn+s5XfcGDQ9rvRzk/w8ocF8hGX6ZTQ3xxrK6QHbyVF83PZcafYKndayqBugHghSRzHD87Y1sWhEhxJAEudcXy7zFpsYg3MvKQNeNsGxIUwcQYSSfDAl1xtMPUt1EtzvKp+jGcIVYGHyapzb0NfPlyiVK4QN0lyURFQ+SE7fzxIY/enCrvi9Pa2wajuevZF1HSxEo7PPN5xPhL2RpSZoIuJZVHR9ORv4o7Bl9J58cDVPKvcpmRjzCwgaCZztLGwJLOrUb4iiFci8Ef6TFnK9TAcvuzGJk2TVhuw7g/rH4gQHWFgiKIqYjj6eNoYZaPo0cOeCNZ26m/HfxVNfCryybMjsZ+JOWleWpoVhxb2Jp8MYPXUP5vtlMhLC1a0qrbSzrcOSAUOIhlgba1WMsV4Sc/ZqlQSHYHICKyyreW+SJaYqCzWF/LH0rSxtNHRAIV1Fbu9MXm9lFhXeYTOfbGeJ3fRJ1+2wJRHGWpcEl5NL0q24Ows+JRELRr79u5oxCEWFpo3kEJLxCO3eE7voi3fAutuqd/6qi5H3ZRPgMm1aEOtrfLA0KpPXbgadmNlOaDGlNz4q9epxLhXJ6rEHnG9ZB6ymE8Bd9HKWF7JpLhm44/FzwBJ9ZFVSwlvPKo6z0keXimyz1Rh1nSYirx3Ye2MATi6YOKAWPL5RhS3lj/0A+FbqIWuIdtDMv8WlHCKEuZ1lFSCrsKijgUTMC/oPugf3eaPqQN5Y5SGHzmC8wxcH+e1haNK8BAT8dmQx9/8GT24psagOxlQWtFHFQnvtYy+GJ/KX0DrVFaxo30EdYCBkmHaGudKO2WygcY2XRegq1gX93jl70bK3wo/cS20taSM+C6f0aSs+0PigUaXIkTRZ6iuNJPq09rKuDNXdA7zC6K69ROPvZROuT1gsdBaOaPgifUXpG9EHpGsk/S4d+iiPmzRcZXzR3HUvDmjjgj2aCdOzwRjPTwlU6STt3N5/SC52dSwXN+9FY/NgARcYsSAp4V49LIQdmWVYRC7Y0WhMHUIgsHfsphx+jwyre6uJDAZ5CUZ3dwRJQuho6UNgT+YSlge6x9g4sQj+8vqSfzSkQwcWL19BiuPvA8Xwi8JsxNoC+px+iBtqQ61kSAP8Calqx1d69gPoAAAAASUVORK5CYII=";
        private static string Icon64PDAPNGBase64 = "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";

        private static string Icon32YHKPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAEaSURBVFhH3ZXBDoIwDIbB+GBy9eK7cDdqvPMuXrzim2E7WjJgsHWWRv0S0g2W/c3fjZaFInVdXyFc+lkaO4paiMTVAQc6fGiaxGoJcixFmqZJLm2sBL9l6RYlcJulWioVR7RvwY2iDjmWSilTBCSnWop2CXQxKQHFICwuuAV/1gukJZCuR5JKIEWzF+j+WLZGvQRSckr23b3AlOP50eFD04Hp+0/Whd6Z9QIQPtBwhGUzailWFB0mCXi2V8/76UVjxywBWIwNxeGPc1kTR/YUEbQGbcKGwsLcXEa2Id7GUUB48X8zOEDZOSHcPJa5BNoveAiDmbF4KPOlb9P3/pzHQPwMbIGXbAvJjJywvIZ8jvg6OswSmFrfUxRv0wmlQdEewlwAAAAASUVORK5CYII=";
        private static string Icon48YHKPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAM4SURBVGhD7ZfLaxNRFMbvnT4Quiu6Fl0JunIpirs+pmqmRYsIoogbEVG6qm3CmCbBlmJBURcWH7QLoT6STmlSi/4J4loQfGxFKQoipbnXc2ZOxpkmmdrmTsaS/KDc75sJyTlzzrl3ylkEDA0N3YDFdFxtaLTWGyXBI1EloIyoWkiSZFNTUzXF0FgVUDl8JepdAaXBq6DhWkjJ8Kkc4sgTqJVt30JRJZCktWYiaSGVbP9daKsD1ayAIpoHWdREkgB8T/M/shLNGdgKKmepcSugioavwGYTUPYWqYqayrdVVA5x5AnUSsPNgCr+u1mKDHcG9HjOYpwfJ8ukFOOFdP91sj70xPwiLqjzqZj9HXDN7evSNS9B9733iqtiz6uJ/k9kbfTR7BWmaXdQS8meFdKxQfsG4LbQWsvqeZI2nGvDx0xrJ1mXvkS2GxY7ePg2fKsMFd3MHvobvPziDR5xE1hODn6XTD4kayOEuE3SRTJ+yxFyLZ82Qu3lo+bjHbKozZJlq2viIEkX3xAXUsZFkgQ/A1XYRwZLeRau7UctuXbavhgiHaJzlnO2F7UQxe434wPf7BseynYh6LFxkjaiKKZJwqe1GVygYd8WUide2NdCQk9Yw7CcRA0xjSxlBpZRr6csAegxHNyvjgM4Pwx9f0RP5MboCpOCXyMZCj2jVhf8yk2yzyGmki6j4kncO5q7zDV+lyz2/WfO5G6yM7CLnCPt4t1JNiJoF4KHc4FrYgJC2wXuYz5l2C1UjbIKIIWMcQ+Wd47DLN3gWYvGUiRDgWvykRM8INlTew2gYgIIr7BFwmOaXEjGPpANH85Husy5TnIVqZrAYtqwIGCLLA7SSttv+U9PH1tk/R/d2jQtxXaoSHWqJoDAr66QRC2sSeMn2dDAkxiW947DIvCYHs/6DlkvgQl4kZy1kQwfwfy7HNcekCojMAE4ut3Sw0zULYF8JrYEbwVPyCJtvfH5OdI+NmqhdpJIK611QWrtV+EB/iILrcRO9Zh4PvgJroC3bTivawJLSf0HROdrJa0oXpJ02UwF6k5hzJiGnfA1WXyIHb3x3H1yNsFDLKNNAGmV/irArnSpz1w44DjG/gDETSZRJzDiTgAAAABJRU5ErkJggg==";
        private static string Icon64YHKPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAATZSURBVHhe7ZhdiBtVFMfvSdZ9Efx6qFKEgiIItoiKSlDRKrYmW0y6UB/9AEFcUWkQBE02ZpOtQmFX/KovVqGCuFQ3iW2y+iBaCmtFQRTxAxSsWpTSJylUNHM8d/LvmMnM5ms7k7vs/CB7z38SZuf+55xz7wwpg8lms8/JUGipYDDWgDAmr4lhNJHAJ68x2YBQMLkEGKHN3NxcINcaZQDGoQirUWmMy4AwJx8kqymBNT95TdQDMA5M0F06WgVCYs1kQFCs+www2YAixkAx1gApKb3PCNwEY3tAWESrwLnqtlEGrFEiAzCuW6JVAOO6xegMkP8R+EsXYzMgjMlrTC6BUN44RT0A48AE3QPCWmWiDMA4MJ13KGiiDAiI1RgQyhuboBnaAEnJUN7YBE0gdXUuiFaBkFgzGRAU0SqA0URCabBxjMaxvLz8SSKR0CV6R+tIRCC4mmAqX/1ahi0tdRb+qF7KbIdYkWSuukCkdkGqeilNE/lDN7BqfoFDNsz8RqOceRjShZzjiJzjNkibsTN8QW1v5i8dp/KVrXLJH9tfALas+cbsziykh1SuclQR3QKpVNO6rr5n51dQ7h5AKv4QwjZomxiThvDlnkLt1vbJy4meQDRSkrnKS+2Ttyw11T55jcuAw6UdX4qnLoc1rHgWoS+xplVGKL9Vp+oz6ZchR8bEdPUBInocUl/XgaXZ9D5IB88qEIvFPFlAiq5J5RenIF2kctVJcfl2SH3G+xCNjGRucQuzegtS4J8apfT9EC48Bhwq3ntcCvVdSAdWMecutyPOvopQx0caxbQng0KH6AAiG+vf8QRCD777gNPxCx9E6CDd8mJpUq61OTVde0Rq/zJIZbHyzZIwSeUW90vGXgspF6WSS8+nTkJ58DXg0+LWM5IFL0I6yGSnEbZgfh2RwPs+LKe/hRgJyWcrjylqK2HmXH02vQTli68Bmno5s1tOcBrSQTrra/aYr87ZB8D4P1Ye4UjYnn8/QTF6BVLX46LMoWvz1qxogIaJZhA6SGd99O7CexukJHbjkPiknqm8MHkKMnR2FRbG4yru1L2sWr+f/8Pf/y/LXej5NCh3+kf50VWQNrKZ+UOMQO3zSdkobWjFbvw2QnJ1J+TP91AuWNH1UmYXQdr02ghZbP0iNR+X67kchxQ1mzcf3jP5OWRXumaAhlj5ZYHT+JjU0wj7g9RGOcGdfp/OyfdDjGKb2icvd//Pfiev6WlAvZx+Wy9vkG6YP2vMZN6EMgLJhktltXKW5l70NEDDyvJkgcZSXELYPxZ/I4bu9fvIt7+1frQ6JJOmduRrd0F2pWcPOIt0/+8k1a6G1H3guDzUbIL0JZSHIeZfSXFclr+NOGQjhv4su78rIVekrwzwh8YQjBbmg2NxuhHKQe7sFWKozqqu9G+APCoiakHiuiHUipkTYoTnkViy6amJwgebIX3p2wBxtOOOG5IBQDY987IfOQbpYDWtdxD6MkAJdEyYzXudRsRPInSQLNgsvSMH6aFvA6TZnIfQxpsRo0c2ZMek+c1DtkGlbYWFSyBcDNIEOybMxhmgkc6flVKQ3aabeHO8gdDF8CVAZOwbZdkN+pQC3ZScrnle9gzfBNnMDNAslTMHZdAfF8S8H6HD8CUgliIykrGYNws08nBXRSgo9R8T1qOFctQLaAAAAABJRU5ErkJggg==";

        private static string Icon32BDPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD0SURBVFhH7ZRLDsIwDEQbTka3bLhL9ggQ+9yFDdtyM0jAqVCVz9h1C5F4UuQ2svIZT2w6Ray1Jx+O7z+MDUUtWJur4xV4hEG/EOwSIDI75+B1JSX4XZnXKsFrg5TM3M0D2q/gTBGmqIC/UdZwHKOVqCmwuOGgEoTbxkFTamh7gI34AJInl8Igi3xKPzXm3LKwD7AqWjKXaNeEWhRru7T8gZoC7N7eHNXntTtcU2Xob5f9nb6hnBxSEw5+0y1950By8AP425g4aGqgOILkTPn3AfgAwWhx0FRPcRbtKCAxGMLXFZA2osDYaJCcHFIFkC4H5HTdE8aAc7U/cXZWAAAAAElFTkSuQmCC";
        private static string Icon48BDPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAN1SURBVGhD7ZjPS1RRFMfveTNB9IuCgkjbtAiyhRAu20SQOsn8CDKoKIIoylYSoeRo4ygYiAZlUVbkoo0unBl0ZihaVH9C1KZoFdIiqcBqTN89nffmjGRNvh9zp6c4H3je7zkzV++Ze865dwThAa2trddo6MpbpaHx+L9RsngDrwJQhlcphCzFwMBASWuo7EABt4W5nHZAWWE6oZJCBZwUpsoi9jyAUlnxKeRVADEeS8ZRCtltlaXmtROc7oAnrXIpVlcbXarTqGyNTqh0Ia8pSwBGOhUedpWNlV/Ebj+lIkVc9Iwod0ErC8ArKueA11TaqNes3hpYLqy6FFL2TUoVtlOoqSu1T5+XteCDnJS4KJVAYE7zwUzVjurn987XzbHbJBBNhgTKLSAgp7OvgA/FJ90vprKx8Bt2OcZ2AI3R5BV683U2i0Jh6QDYnY6Hu9llBPCQhjN5qziI+B4Bb2bjkRvsso3SGgAQPvoZC0THL7LLFgCwSxPaYCCaeMQu27gOAFDbnY6HYP3M7DqB4hi7TVBoJ1j+hTFndk7fSml4kt6ZYDcDpymIA2zYouQdGBts/pHuCY2STOc9tAwh9rIsyrO+I9PZ3vBjSrUIFdNddjNwloUtFKYQbmJh8JFHS7Q5XzsF8Xt9Hz/aNbqBtSVKAghcTTbQJ7efTULeZ2HJZF/TZwo+yabJN92/k6Ul7mtA6gcbOxKnGjvGY/RbMuw2CmA4HY/0s2ULSrnXLE0kivIHIH1wh7rHCIDWyS6jC12mejjHpm0Q4StLEx9o1SwtUVgDxkJEf2NnqoVN29C87yxNpNBsr8t9AKjX+DWsQgmX6CBayGFAvBWIphZ2xQ0o5BeWlrivAbFmPhULT2V6g0OZnnCYghjhlwiMNbQnatiwBEBuZ5kHNCpseyhLIU3K2yxNwA/NLG0AtSyMfMKNWu4lW5YoC0AHMc0yDwhbO3C4bYIueqKeTYMXY7Hmn6wtUbcD4A+yNAEJb1n+k4b29Db068OUQ2vZZVxDHF3oXN9GNQEtiPKDFGIz/dk91E7b+CUTKaE+2xt88udtlIo8RMNWmldHY4Ra8UL+UzeazPSEmti0hesdkAKHECDJZ8GixdPVIGYsns1FGHPoeUDzLpS6eAMlKWTeZRDf0fOU9KFMPGT8m9EK4/B6RbMnpICgm8ULIcQvUiA6YZu67i4AAAAASUVORK5CYII=";
        private static string Icon64BDPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAATqSURBVHhe7ZhRaFtlFID/898s2RzoiwhiZSi6CaJIFaUvTpFtbVqbdrqh6KCCKDKUUXwQmyymzVBf2geZD/ogIqIrkyW4JH3wofpgtXMykDmZSHXqw0CdTubWxfzHc3NPb5c2y3L/3Ht7a+4HN/85/59c/nPu+c85uSACzPDw8Cs0pC3NGwLrAD+MN5E8BhHPjTcJsgN8IchHAFmsMj4+7slewwjg0VPcSGirNgL8yua6+HEEAmu8SZgDePQM3WweVgGfWDUR4BVtHwFBdkCGR08JrAPoqJj9g+dOaCkH6DQ5XmVzXbQjQMf4INLKEVj1xpu0fRXQPo/N1nfdPsAv2j4CQgfw2Lb4ngOWstI5IYyAKz2hZmkQAQ0bpv99BJCBvvT0ungeAUEnzAE8OqbZKhB0wkaIx7YldACPbUtYBXh0jFv9w0oTHgEedQhse+sEg0fHzMzMTHd1dZlH6AFrZnWinQO2vpS/BWLqX1aXsUYYioZ/fjt74dwXEzvPW7P12TKSv0lG1GVzSmR+zV+F1/rOsOoq2g6Ip/JOkuDXAmGqmO0fYd2mO5XfQ+dwgtUG4DwKOCFQfIIAk1Nj/Ud4oSX8SoKdAvDlnlTuq83pdITnHAIxelp3AYgXpcBZutc7vNASvlYBEHD3+krnm6y2BN1rqGdvfpJVbVxzAKK4WBxLgHmd7rghijKyCVGN8rINCnxiR3oyympdlBJD5n2khA1KgJlk37dWagEUO7pThx5iVQtPIuDos/eUS5nek6XsYJocU+DpKgBw1d+V6O2sNuRwpv8UnfVPyRlPkjpgzdYCQj7NohaeHwEAvIbFRUBezVLTkBPyiLiHVRvKC4/FRw5vYNUxnjsAEc6xuIgs/8CSI+41Em/QGfqDVRslKnEWHeOpA+Ijufspa29jdYEDU5lHfmHZEZkMKAQ8yKoNVYUbWXSMZw6IJ/M7EURtqULxo0CjtRYaxbcs2aCEDhYd46oDyOjpnmTuKF2n6XAeoIR3My/RvsWsqmBvMdt3gqe0oA3PsWhDeWDlI4BCPUo72UxGd9J1HU9Xobj9oDSWuG/q1YFlT88pFcDlOSUIDmiEBHycOrePtiXzTZW/RhgIyzI+VQftlt41B5iNECWoZ0gYJtns9r6xViyoc9suQRyJp3IP8pQWZOpxFm0o+v5k0TGuRkBpdODtYnZgopRN7Ka6fSdtbIiXqtBjWkefhZ50YSNPuUUwHLCUwmjiXbL6BVYXWAeV8vMsO0YpuI3FSwCtsmrieQ4ojlablwusMtBC+4p3sLAIiM9YcowvSZA2fZEFCxBre5Mf38qaI+gP0lYWbUDJYDuAeoBaBxBowHoWm6Y7mXuUhpoIoAowW8g+/D2rjvHcAfG9h3ZRX3AtqxYoVDHTd4y1pqAma4g2+x6rNlRdXmdRC+36ufSVmFkGK8b89aasRHRtrKw6UMotVKOy1S9cgvkXmSpFnynXeyWmhNqtZPlDWY5tlAZ00i/MRLrJWl2ENjBNDVZLZdW1CDA7wYiK/W5eUQW/omF8Wdd4gWeiqJ5jtS5SyP3mfaQhZugX+2mqnvGfR6UaZFUbX3LAAmT8cQrZp/L7Bn/mKV0OkvG9+cygdv1fwLUjcBnOk9Vz9MU5MnymmO3fx/M2V3wrjHiWIuknOjanAPA7lOqtUmb7SV5tESH+A5LrnDhPBY0gAAAAAElFTkSuQmCC";

        private static string Icon32CoalPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEkSURBVFhH1ZXNDsIgDMe3xQdzVy++y+5Gjfe9ixev881mi+2ysa58DAn+ElJGgQL/MuoqA13X3cBcv19LGrK/RgyeDTiBEQt9LlAl0I4uhr7vV/FcEpR7dCHskcAMko4uBG0TuW7BnWwY2tGlovYJsFcCjVwSxJFFArIiHDzBLfjTtyCVBNo8XhKkIuYtiPuBlEa0BKnQpCz7LciBkeB0eR7BDFgn2tfj/Ka6049YfezxRgJoW98CYXJkoHYpODL5EaHPwq+BOWAG4uq44DfAE6r+eXDb77OIFEnIC23JIlxn3ybiAqydeAH9J83ndRe5ruEmyRYAeo/zQs1OyjyB0F0gnDdcqNnJAQpmLF4ZOyBnssvvjbSphjLWnmz6k7n8+6iqD26vrmifvc54AAAAAElFTkSuQmCC";
        private static string Icon48CoalPNGBase64 = "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";
        private static string Icon64CoalPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAd7SURBVHhe7ZhpjBRFFMfrVe8CBoR4IWAiJkQxJkJQUMEAEjDMATu7wIIxfPCAaGIQQiAcO8M4GQbwQjCK6Ac/GAlkCcsMxyyHBsHIKgoIMYZEo1HkNiDEI7LbVb6qftMz3TOsM70eyzC/pKf+r7q6p+vVq1fVDayMmDt37gtYRC2rOMrGAV46r+BUlgMld15RTg7wRDlNAUlSs3LlyqL6VokAKj3hNfH8F/zrEdCZO18KHZkCV33nFZUcQGXJeM26/xaVVcAjZRsBxXLNR0A5OSBGZUmUjQNwCqp9SclOKJsc4JXKKuA1e7qpRMBVSsUBVF6zVFYBKq9ZyioC8JlK/khTNhHgpfOKcpoCnr5QVXIAlSXT2XKA1+epRACVJeP2eGejEgFF0hEHePoC09nw7AAMMU9fYDob/2vm/ieprAIeKdsIKJbKKkBlOeApIRtUXvW0tLR8NHz4cDWlH7FqKhRFu0kw2JAcJww+EKS4Dc1+TLK+EuA8lufxynMS5DdVwA9ui9Ucs65w4l+cHIoxNgWY7It/1QeP01KykwAymY7XfkbN/hZfNDVICrM7mZqd8UktJPPwR1IPk9QIs+27ncsmnyLTQUEH+JckZzMJT+LJwVTVLmfOnO5y8J1nWsnMdDwODHxUlYeU8mMwWDgdq91HVVckEE59jk86lEzNMF5jxGIgyHQQiKQcKwI6fUnz0lCcTAeuJCjBH0luAgmriu28G9/i1EPAWUt7nVcAwEgmYG8wsnk8VRXEt6jxFnfnFQfMZA3JDuFwAHp6Dz74JDIznJLA5pnMHME59BfCHIkP9DjWr0bPXraaZOGcrcfeVZGJPmVfCyGXC5MNN4UZQfsLOqMRDNb7ZqW7kpkHVHebStIJ8ImkOoT9oIFIMox3HU0moeeqckhuSP1I5Xo85uB1YzLh729IzkTn3KE0cdGQbEI6Ufs92Z8GF257U1SbhzHC+qsKdPgNrNflZ1GuVnYeQk7DRnkA3pdkh9AR4I9uvwtv6ZgjOLqfYefrlLRqCoNt9pDEQYFZJDUYOa9uTYQynddsXzHhAnbqZTItgCkH5FEfbewBwEaS6QRYb19D0wCyPKMdAKZZry0HsIJEUfjCycewuNeyLMBsfY+kAykuN5LUYBTcHVySynuG39q6TiOphuEcKRvgPETSM9oBuJw5Egpm6GPNS2uSZBYFZzCMpEYyeSadmPIDmQ52LJ96Dv8jM5UsJNxPKgtg+GcAoV6/3XQ4D/BAw7b+OMUeIJuAXSSKB5jaK9jgHD1KsiAY2t+S1Aim9xo29Y2NBjZ6lEycanVrcC4eIFODkdPhXR83+eV+pHMQRW9ScnB0ADdMf5AsjIQLpCwkOK7/9Wg3e/Qxmk6oEoR4V1fkMCGyZSxJT3BDGrhLc8INOEuyBKQ7Atp1AHbqF5IajAjn9UxMJ4mavaTKdKLubV2Rg2BSJWrPcJz/vUlnkbg0lQiuGreStAD2J6nCSHmJFCFd14OfFOvOe9kjr3ILSQ3mkg7lAY7zqOAeuWSA6TDNgA65jmRhuMvJEuzrg5Fk7qge3xgb8ytpjCxYR1KDO8rbxy1o7EVmyXBcSvIcgF4dSLIUHA7AsO1GsjCSuaLMmucK3B0+QRKR75DQYMi/T9Kmurq6wDJeHLy1rS3fAYyV7AAcGYcD8C49SRQEHXQzSQ1umuzr8VzOsgxx9XKTOTjAITqRA0wmUTJ8V6LuOA75J2RrcFpMH794U15ybJ/sCFqAY1PkBleJQSQJy4H+SJM994sFp0G7L17tQRsh2KKtHAxeNY9kUQgpPiRpAexGf7QJt9j5+KLJe3CUryczw171A9J4WlsWafuQsjl7sGZ9NodgeKunt1ftANwMrMPs6lyXgc0NRrYWvcTsSEzahYnP+W4vjBmkHIDJZpIk5KfN8dBuLYHZ4Wy0safS8VBQH0trA9kjFKAmNgKEpzygHbB92cQTOIef1zU5SCaa8G1vIZkORkf3VAXDqQU4L88qrSsBXtclgaM8PxDdPIJMTaAhOQpDdg6ZGiFgrSp9OZsaHJCDW5eHHEueA/e0ldKTA/AZswQaUqvQJbPJtNHRIWE/yircsGDOYHdi7YPYYZ3pf+M9q/fGxrQp7Q+n9uW+welvBhx2M2nuZ5L/CUwm8Dr7/R9XnC+bl9YOUToQTm7Ac7QDFPNx+/uKpfPB/1mE/7OMTA1Giu6P+4sQPq+9jLrREZAhnQjhyMBzZNqod3b8syAe6uvNDKwYnem8G8OASeiwI2RiM9YFRycIjCdQv5LbeeSwEGY2nO3OY/d51w0kCyINtp2kjT+8pZakE2A9rnQ4HKBIx2vWCM4G49C8haNT8JtbBnTzV9jujczoK7bFan5ujtcOwXOv6fG/ElKsTfNDQzMfKwPR5Chdb3FkRyzwE+mC7IiFjuLNHdtt8JAHHFPAzdiFTTd1qTbuw6cdgFNgAIC8iP9yCvt1uo13a9kV852npgWpj+7p8bu8pJJaH2myvrjxPi2lcbK19Y/UBy9OvWi1ylIfbexCkm2MTc373OYmt30GdV2h+lyy92bsLzIRnYOUxsewAAAAAElFTkSuQmCC";

        private static string Icon32NJ2PNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJDSURBVFhH3ZXPSxtBFMc3s26C6KnQVs8ezCGePPRSaTXtQQP6R3gOhJCrgtpbQgjkL0lpYynVVvTgxZMe1LOQkEBPRVmS7Pp9k7chvzQzYYLQDzzm7czsvO/szHtrmSSdTp+S8aMSgltTvGdTxrQAbab4k00LITZyuVyt3W2eTCbzxvO8MtyHfD6/0u5tf4EIbBmDRzRJ9hqGgx/BXYZRvA6i0Wgk0F7CYph0kkql5uWIIZLJ5Gus+wtuDHZt2/amHGBEsVisQ0QcPomI4iiOTYmg4I7j0M6XYBR8NZvNVuUgIy/hJESoBCc6WWBShGpwoicNSQQCf4J7BSMRP8e5mAh+jIaCX2GND08FJ0Lc9hDs4J/9aqkSWWy2LKfuC7F9uL95yFMGSOyWErbnfgthybfujTXb+vvszgOGCiBo57eRd3dNEXa4SxnHd62F+3N+epazF6+EI4+gGok2W6Gpmu+Htstftn7wlAHWd7+uhz0Xlc635txbawZHgPxfKxQKFZ4ylAEBXVWLCgddorhqiUZZ99mlS6z0fs8R0M77qtbncf4PSOc1NErVtSNAJ3dHoVNTpACTwQNURYhJBA9QESEQ/Dtapao1DkOqa0kOMHQELuwCA8q3XRdal9aHewGjeJOB0rArFZV48UpoWsAZ23/Ixk6p52z7nwnqC4y7RqJ1BFj4I7tDKR9syX9L0Kqgewd+jxKhi66AVZhREVoC8Gn/oJEiZIcBtNOwS0QHnUvXj7YAgkV0s0ciWMheu0sFy3oEUpQvB0NDe60AAAAASUVORK5CYII=";
        private static string Icon48NJ2PNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAPvSURBVGhD1ZnPaxNBFMd3N6WimFBa9C7Uu4ii4KFKtf7CHyD2oh4sFUS9pM2poDFVb2lyURGtFfx1sAdR8Ve11puo0PoHSCt6qYhFG6lp2uz6fZu3YbNJzGZ3nMQPvM57s5PN+07mzU4a1TAMRSY9PT2tqqqOkI/37kgkEh/NCx7RuJWGpmmdaFaRse8L6QIw643sFvhekS5ANBqtSVgvtdxXV6Be1N7e3sNk5HN3Hg19z2Fx2NtwOLyW++uCWCxGEzwI9xYZiTAv2LAvoWYU1Yt6EUHJz87OXoPblespjabr+kG0M7mwPkSUSX4oGAzeYT+PlkwmxyGiHUvoO/eRiNFIJLKeY6kgDzWVSl2Cm08eXTdCodCxaDSqc1cecwlBxIdsNrvVJqIJW9yIbBGUPNb5Zbz3ce4yk8fMd5dKnsjXQK1FeEmeKHgO1EqE1+SJogeZbBF+kieKBBCyRPhNnigpgGARHXCtLZZEPBO5xeJ+p+zJA9oqXSdPVDxOI+E1gUDgJca1pLWgMr1k9eK8tnzBUNUJLauHH53f/46HVqQzNtyozqdepQOhTRSHFqeVFZlPiqoYVc+8havvAzTremDp6NSydU1ZpYF7TX6rhppAm8qFf0dXjZ04zLRxaNK88FlZmZn6BjcJc5088qax911/oTnQd/NCuqGpj0NhNBgZpXXuDUdVM1m2BpzgI/5HX9383dbvEkrrhnFd1ZRfHFdAbcM0bOTApGXhC+pg8icm6CovC1dg7Byau1UX8VcUMdoMXjWhK2r4af+e9zy0ImYRZ1DEmqQippmn0ync5lyPMoOD3zZsseMcVw32/bNoornI3KKtexND5Q5t5ShbAzzz9N8D6w1+YJZ2+EneCe53EXaFQ6ILJ9FBOk5zXJGSA+3Lhrso+Y54PO56ubgB9zcGBgZO2EWg62g1IooGyUrewq+IggGyk7fwIyJ/sVbJW3gVYV6odfIWXkRoSJ62ylFb8rRVtstO3oJE4HlwEu5Qricngr7klxKB3LVhtML2eRHQc4CeB3DzIgBtsYfYz2NXVBfJW5QRUYSGj2c7LALbUC/JW5CIRCLRDfcIGerjtnnBhuvjtCgcR4kYkqLYM2W3p/8F6QKwRWbYLfC9Il0ANop7aKbI2PeF9BoQjS8Bu888NF/8uH9vwQ8PlfqdOMcRWF70AOOoPEKWEBLbzK50RNXAmBsRNNP22XbGXhBZxK5EiEb0LiRdBP3IZxaMF7OxhVtijNuSryGzKHXNskrXLRPyCTw5t+81GrsIaQhbQrUSIbQGSonYdfqBQcahcEQXsSXCToz+WEJsYsx+vwgX4ASC6LjsTDbG/T5RlD89J4OrT57TJQAAAABJRU5ErkJggg==";
        private static string Icon64NJ2PNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAARVSURBVHhe7ZrPT9RAFMen3QUF4gENHiQmmhhNvKiJf4CcjEog/tZIIje4qAnLwYNgIh5ZEqMHPeEBRI3GQBT1YPRivHjwZOLB+As8qMhBXGXpbn2vfUO6s+3udjut0yyf5GXmbXfb+X47P9oBzTRNpiKpVKodiht2xnrS6fRDqktFp1JFrkNsoMB6KKhsQCuViLMuFZUNiIQVA6isWbS+vj7nbHsWZtv7VP+vwCpQsDxBuzSqVgToOqhp2jVKPVcR7AHO2fY2XPg4lLEGNYD4u1Atu4qIQyAJMRZnE6jtYxCopSxowDkIw8psYmuCh3gDHvbOUL0IHcc8dJeTUBdNGIcTdtmp+kBbj0Ihis+Btu6RkZEHlBdhDYHh4eF7LiYkIG7GwQQSfwtCFH8atI1T7sryHBBXE4KIRwomwbiZEFQ8Iq4CsTFBhnikyABEdRNkiUdcDUBUNUGmeMTTAEQ1E2SLR0oagKhiQhjikbIGIGgCFChWNGEUGhbVE6MoHttyKoh4xNeeYH9//xH4/gRUrYYsaavYt/ot5q/kukU41RJj5rO8qZ1/PNTxDo9Xy94Lkxub8z8+/06utfKm3DxryX5g9fmMlQOB7zzH96YoNwHEJz827IaWOG+KxbyRN3c8vdz5hXJfoPikrr8BM231RAJu+KbMa1ZnLkoTj1S1K4zdfqZ++8RCXYuvd/SgrDG+s9bFt5QF5itET9Xb4u2DkxmTaQ2URoJuGmxr5iVlUpitaBJUBej6VJNHVQbgEGgy5lZTGhmNxhzVpIBDoNf3EODrcVZvTH5q2MVyBSsTTF2MzbGEsXP64qEZ+sgX+wamtukaewXVZvsTG91cYpv/4CSYzUHanU6n8d0/ML4MEB9G+DK4kFj719R0WAbZEzNhpKoVz1leBhO2B025n2x99j2Kt3JAmgkVG0APPEXbTRBd0JA7dioPuJ7YMLyW9GtXNAfQnXfdbgpDvBtwLfFxHNsSeNuurAFityekPoxUAj6Ou5gQ+J2kpAGqiOeEYYKnAaqJ58g2wdUAVcVzZJpQZIDq4jmyTCgwIC7iOTJMWDYgbuI5QU2wDIireE4QE3T4wmEoRfF4osDbTVGCJkCBYkUTRvF/Bey0GOwBVyBE8aE83oYNtVk0IQm94yrVixBXgdiK53iY4Aka0AuB78YYJ+IsnoMa4CXvGFS5LtToStVbYmEDc1NBw0BUKPuP4hCoOVYMoLJmUdmAWSoRZ10qKhvgXJ08Z/GgKLsKREXNzwHSe8CBwSnrhI8udbiu2+WOc/j3RMr9zi8rqwCV0oE7uIeqVYF32nm3xVwWYfaA50FNiIKwh4DyJkQxByhtgq5pmvV3d1nhoI1KxDLBeVz8nVdw3I7JiNB6wPRQ5wsoCkzYPzCpXE8IdQi4mUClMoQ+B7iYoBRRTIIVmQDDw8SgNDIiMQBRtSdEZgBCJnhhmcN7gtAjQjMuUgNKUaKHtJUxLgCM/QOLfDhlJspRwwAAAABJRU5ErkJggg==";

        private static string Icon32UPGMAPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE1SURBVFhH7ZVLEoIwDIbB8WC6deNd2DvKuOcubtzizbB/STq1FPogIsz4zZQ0faRpmpayEKSqqpsS116LY0dSiqTFxVER6FBIjUI6Aslk58DUeTdNE213TgREznv2EWC3XKgpiU3ngM52e+epNwBIR6AmGc0gAlPZ7SP37BlfBLbxmsWOC/HzW7C+a8hhDSWXRPhBOWYowoHkf7+PbAe+BhySCm8M/1uw3mu4FL4IJP/RNk15ujy65/1sjsLWUdeNPbVqx+OD9oMSLerEUfW9UKE5Wudxjn2eq8cEkxCTyYD96mmjVp/tDGg9Tn4sDgld5BaQEy6uU8DsHBJ60AHlZYeiqoPk5D7qt4FxWxpocSP3+EwxsjsN97kOkHHugzC4Yxd/iOA0F+iIQO14FfMO5MzxUBRvfZ2f2PiEkNcAAAAASUVORK5CYII=";
        private static string Icon48UPGMAPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAG6SURBVGhD7Ze9bsQgDICTa18qW2/t0nfJXrVV97xLl665LY/VYs5GJPwEDgOJxCed+JExNjbm0neVGMfxUzQf99HjVHGAy3jggm1pWIwHakXgD7vdNE1JNtSKABundyBbCoVe1EOmEGeV2SNXChUxHsiSQntVplUhjVaFbMSkUCq1IvCFbTJVHBBRgTLL4oQ3hTjqeWqV2cMZAQ7jS+BLocMbDzjDm/LYpKyN5fTvQHOgNqd3IPslzk0fstkDDpz7g0Y4zPZXYY8sEShJljtQklZGa9McqM3pHQiqQkfGF4EiD1EqT9gaLMtyG4YBIvRyn2lkoX99/5G5/vv9troP23kaW7gKmRv2JUIWojbfR4qVXOi+xEan0sVRhWZULnEYD6zkCMtaA4tOpetZDiNwnBgop3m5UYAcAcZcqY+tQjeedJIuWHeUdwAMNIxHaF6vitSfizkAp7eNCmIzzECshW8Mid6PdgDCp/9wmlLAYCOvNiZchoXCEQGjCnmwfmaK9a7oKDYHQQcX7wBtpv28xpMcDtk5yiXeRTuw1YGAAzJ/HSFy5rYHbn1eLsIbSAGb4pjcVnDr89N1/ywZ+B6izRaNAAAAAElFTkSuQmCC";
        private static string Icon64UPGMAPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIjSURBVHhe7ZnJcoMwDIbT5aVya6699F1y76SZ3nmXXnpNbjxWq98RGYgNmEVGYH0zjELwJlmLQ552ijkej18kTrc7GdQaIIXy4JmlRsSVB5oNkATNIfDHHx1FUYisNXsPMAOwzJZFcsCYEreZHJCqvseyRAioUR5YDmCZjNj6bueARJgBWGbLanKAFBYCLDVyZimKWgNQaODEKG6EqBwgeXyVqu+x9HqApPIaiAmBzSoPsq8CvfE395l87vGmkr0HmAFYZosZgGW2LF4FlsY8YOiOzOABeb8WJwMm+ZUXS3IP0IblAJatzF0FtGEHIZbZYgZgmS1mAJbZkr0BBp8DtoaFAMsu1PxwkeCFZStlWV73+z1C5e32jWFsCVcF3j9/XKb//f4IVoXQ8+q7Fs7UFi8+GlAfhNHldudxoD5X/uyozeE9A/XxOtZen9MbR6oKnGjihgF6lAcXbhPCexYxXqiNN84ry9GELE+TYOfw3q9uhL6dQh+0CT4nsHi3gwHFPDra3MfBzdrOAdUODlX+wFfF3RPUGACe0eYdRGPxLEH9+zqNNtht3nFvnFV4QGDxIJgYH2i0CY0zOQeQK7VVg84TZFs/WmTQC7B46oPFY+dilK8UbsDj8J2cBwTL4FSweLoQKr3KxyJSBWJ47NfhSaOJGXNtVWB2Nm0ADhfv4scO8wCWLmMjZkKXazH9vUCKOQbjDEBu0feP7eSsnmKO4ex2/wY/DrqQcrybAAAAAElFTkSuQmCC";

        private static string Icon32AlignmentPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAkUExURdVeAACec8O6AAByst+GQP///0C2ltLLQIDOueHcgHJycgAAAOjJALwAAAAMdFJOU///////////////ABLfzs4AAAAJcEhZcwAADsMAAA7DAcdvqGQAAADMSURBVDhPnZJbFoQgDEMFqS/2v99JmyrI4DiaDz3CJW0jQ84DFahIjRR2/wKSJACTiEwhznjNccRTxAHsgwiC8xKi4LyogxwO2E1CACUUsBIVgA5qAN4NoDo5tEBxuOiBwAKfJcQVr/UMUFbgbVAmX3oHWNQe7nXU4LXv7pgWdQ10oy4AvBtA9bNE43ADXEaNT5W1+DAoX/J/kalnAMP1W70DOrpH7YP5jdqBagoDUIJ3sg/A7QA21bdDAboONz0wXL/VPYCyAm0OOX8AMXsRATc2+9MAAAAASUVORK5CYII=";
        private static string Icon48AlignmentPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAMAAABg3Am1AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAkUExURdVeAACec8O6AAByst+GQP///0C2ltLLQOqugOHcgHJycgAAAO6eux8AAAAMdFJOU///////////////ABLfzs4AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFcSURBVEhLrZbRbsIwEAQNxYWW///f3s6tYydKwEGdh0t02hHn2Ikoz5OUkcvAdeBrwNHEWXAWnAWCt1pvFr5r8J3CXfd3BN2JFCKfhvMYzmNshMiXgnG51KpxVK/XWjWOKrMoHYRQ66OUh6xpIQq1CaIJguScEGP/7AjJgbA3UrI/0q6wHUl1Ujjah2BfONrpoAsdjdPQOA2SxtHEWXAWnAVHE/fAWXALHE3cA2fBLSDYjrdWliC8P95rYeJ485t+fC83rh1vOjNCFCqdjSBozAkx9qvjTWcr7I2U/NMaTggTx5vOIkwcbzpG4zTcAkcT98BZcAuKe+BXAZjfOAsfCcviIre807o2tOIEoT++8auRN8lG6BuUX6SsTgCz+PK5IJognAWSayG6cYzXwq85EoaRxBsh61rokHwlqDoLJLswsw9BF2Z2OhiEBY3TcBZImvOC/6RMc1J4Pv8AbNwfPx2ok84AAAAASUVORK5CYII=";
        private static string Icon64AlignmentPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAkUExURdVeAACec8O6AAByst+GQP///0C2ltLLQIDOueHcgHJycgAAAOjJALwAAAAMdFJOU///////////////ABLfzs4AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHISURBVFhH3ZbRUsIwFAVViID+//96T7L3JKkFLUNlxn1o6HV22zqQ6cuS1wVvCw4L0Dp4Bs/gGbQOnsEzeAbtWMqxfZL0XirvGTi18xOBdiYcCD8L3W+F7rfCaqD6FMIpRZdmDacUXZo1HFFlkf6xHu4MNFWFOVBJMcFbBOrNR+HugI6s4awF4vaC/QLJ9YDYM6Dj90eoa4q54u0bOPNA5dwCF07L5ZeBLMhXIAvyVwMTkkYkjeAZtA6ewTN4Bq2DZ/AMnkHr4Bk8g2fQOswNnmFs0DrMDZ5hbNA6zA2eYWzQOswNnmFs0GIzaXuqAu1rI+JE0qZdeSWwbVeOmfBXNZwff0zyvSsz3xRoqgq3AxXGi0C9+SjcHdCRlfkyELcX7BdIrgfEngEdbz+CeF5g267MfAxs25WZG0kjjA1ah7nBM4wNWoe5wTOMDVqHucEzjM0Lc8Nubj4X4Jn/Ehh3XUnjuzKeCaf9VRCYdt3utwKeWQtMP5ZwplccPBOO6F/lFHNNMVc8g7caqKSY4Bm8xwfaP2U98GH2DIhrgQm8vwnUFc/gPTYw7brhTO/KeAZvCky7ri49vivjGbw5MCJpBM/gmWcHDocvmwAy9eye7+cAAAAASUVORK5CYII=";

        private static string Icon32DistMatPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAMUExURXJycqS91////wAAAMMQ0YYAAAAEdFJOU////wBAKqn0AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAASUlEQVQ4T83NQQoAIAhEUWvuf+cQxoVSNkvfRshPGj4MFvhSSMFy04OMiyAF270uTQmuuNcCnuAol6YEHS3g33nEpSlBRwlawAFvAQZ3dyFgOAAAAABJRU5ErkJggg==";
        private static string Icon48DistMatPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAMAAABg3Am1AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAMUExURXJycqS91////wAAAMMQ0YYAAAAEdFJOU////wBAKqn0AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAWElEQVRIS+3OywoAIAhEUcv//+coJgjp5axceFcRHhhRZ6Kyht9LFCizBIeCgn04sFGgzn7WJYgDnuF2RAEM6pPwuKxLEAd44gBWmEl42HUJ4gBPDHCl2gA0vg/HOW9duQAAAABJRU5ErkJggg==";
        private static string Icon64DistMatPNGBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAMUExURXJycqS91////wAAAMMQ0YYAAAAEdFJOU////wBAKqn0AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAgElEQVRYR+3QQQrAIAxEUVvvf+c24SOCaFNctOi8XUwyYFKelFp0gliq0QliqUYnyDaOigIUYHYNGGOwh6kBBnuYGmCwx0bOyl1yDacABZhtAl5hr+A5jr2C5zj2Cnvj+64tuYZTgALMugEzfhLAf91jyTXcXSpAASsFzPg8IOUL3eYaULkuDZsAAAAASUVORK5CYII=";

        private static VectSharp.Page GetUniformIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32UniformPNGBase64, ref Icon48UniformPNGBase64, ref Icon64UniformPNGBase64, 32);
        }



        private static VectSharp.Page GetGammaIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32GammaPNGBase64, ref Icon48GammaPNGBase64, ref Icon64GammaPNGBase64, 32);
        }

        private static VectSharp.Page GetExponentialIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32ExponentialPNGBase64, ref Icon48ExponentialPNGBase64, ref Icon64ExponentialPNGBase64, 32);
        }

        private static VectSharp.Page GetNJIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32NJPNGBase64, ref Icon48NJPNGBase64, ref Icon64NJPNGBase64, 32);
        }

        private static VectSharp.Page GetRandomTreeIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32RandomTreePNGBase64, ref Icon48RandomTreePNGBase64, ref Icon64RandomTreePNGBase64, 32);
        }

        private static VectSharp.Page GetPDAIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32PDAPNGBase64, ref Icon48PDAPNGBase64, ref Icon64PDAPNGBase64, 32);
        }

        private static VectSharp.Page GetYHKIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32YHKPNGBase64, ref Icon48YHKPNGBase64, ref Icon64YHKPNGBase64, 32);
        }

        private static VectSharp.Page GetBDIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32BDPNGBase64, ref Icon48BDPNGBase64, ref Icon64BDPNGBase64, 32);
        }

        private static VectSharp.Page GetCoalIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32CoalPNGBase64, ref Icon48CoalPNGBase64, ref Icon64CoalPNGBase64, 32);
        }

        private static VectSharp.Page GetNJ2Icon(double scaling)
        {
            return GetIcon(scaling, ref Icon32NJ2PNGBase64, ref Icon48NJ2PNGBase64, ref Icon64NJ2PNGBase64, 32);
        }

        private static VectSharp.Page GetUPGMAIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32UPGMAPNGBase64, ref Icon48UPGMAPNGBase64, ref Icon64UPGMAPNGBase64, 32);
        }

        private static VectSharp.Page GetAlignmentIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32AlignmentPNGBase64, ref Icon48AlignmentPNGBase64, ref Icon64AlignmentPNGBase64, 32);
        }

        private static VectSharp.Page GetDistMatIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32DistMatPNGBase64, ref Icon48DistMatPNGBase64, ref Icon64DistMatPNGBase64, 32);
        }

        private static VectSharp.Page GetIcon(double scaling, ref string icon32, ref string icon48, ref string icon64, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon32);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon48);
            }
            else
            {
                bytes = Convert.FromBase64String(icon64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            VectSharp.RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            VectSharp.Page pag = new VectSharp.Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }


        public static readonly StyledProperty<int> SelectedModelProperty = AvaloniaProperty.Register<NewPage, int>(nameof(SelectedModel), -1);

        public int SelectedModel
        {
            get { return GetValue(SelectedModelProperty); }
            set { SetValue(SelectedModelProperty, value); }
        }

        public static readonly StyledProperty<int> SelectedDistributionProperty = AvaloniaProperty.Register<NewPage, int>(nameof(SelectedDistribution), -1);

        public int SelectedDistribution
        {
            get { return GetValue(SelectedDistributionProperty); }
            set { SetValue(SelectedDistributionProperty, value); }
        }

        public static readonly StyledProperty<int> SelectedMethodProperty = AvaloniaProperty.Register<NewPage, int>(nameof(SelectedMethod), -1);

        public int SelectedMethod
        {
            get { return GetValue(SelectedMethodProperty); }
            set { SetValue(SelectedMethodProperty, value); }
        }

        public static readonly StyledProperty<int> SelectedInputTypeProperty = AvaloniaProperty.Register<NewPage, int>(nameof(SelectedInputType), -1);

        public int SelectedInputType
        {
            get { return GetValue(SelectedInputTypeProperty); }
            set { SetValue(SelectedInputTypeProperty, value); }
        }

        public NewPage() : base("New tree")
        {
            RibbonFilePageContentTabbedWithButtons content = new RibbonFilePageContentTabbedWithButtons(new List<(string, string, Control, Control)>()
            {
                ("Neighbor-joining/UPGMA tree", "Build a tree from a sequence alignment", new DPIAwareBox(GetNJIcon) { Width = 32, Height = 32 }, GetNJPage()),
                ("Random tree", "Generate a random tree",  new DPIAwareBox(GetRandomTreeIcon) { Width = 32, Height = 32 }, GetRandomPage()),
            });

            this.SelectedModel = 0;
            this.SelectedDistribution = 0;
            this.SelectedMethod = 0;
            this.SelectedInputType = 0;

            this.PageContent = content;

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == NewPage.IsVisibleProperty)
                {
                    content.SelectedIndex = 0;
                }
            };
        }

        private Control GetNJPage()
        {
            Grid mainContainer = new Grid() { Margin = new Avalonia.Thickness(25, 0, 0, 0) };
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star) { MaxHeight = 275 });
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            mainContainer.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Neighbor-joining/UPGMA tree" });

            StackPanel descriptionPanel = new StackPanel();


            descriptionPanel.Children.Add(new TextBlock()
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Text = "The neighbor-joining and UPGMA methods can be used to build a tree from a sequence alignment or a distance matrix.",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            });


            Grid.SetRow(descriptionPanel, 1);
            mainContainer.Children.Add(descriptionPanel);

            Grid modelGrid = new Grid();
            modelGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            modelGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            TreeNode constraint = null;

            List<Button> modelButtons = new List<Button>();

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Neighbor-joining", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "Creates unrooted trees", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetNJ2Icon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedMethod = 0;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "UPGMA", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "Creates rooted trees with clock-like branches", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetUPGMAIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetColumn(brd, 1);

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedMethod = 1;
                };
            }

            Grid.SetRow(modelGrid, 2);
            mainContainer.Children.Add(modelGrid);

            ScrollViewer optionsScroller = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, AllowAutoHide = false, Padding = new Thickness(0, 0, 18, 0), Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(optionsScroller, 3);
            mainContainer.Children.Add(optionsScroller);

            StackPanel optionsPanel = new StackPanel();
            optionsScroller.Content = optionsPanel;

            Grid inputDataGrid = new Grid() { Opacity = 1, RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity };
            inputDataGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            inputDataGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            inputDataGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            optionsPanel.Children.Add(inputDataGrid);

            inputDataGrid.Children.Add(new TextBlock() { Text = "Input data type:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });

            Grid inputDataOptionsGrid = new Grid();
            inputDataOptionsGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            inputDataOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            inputDataOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            inputDataOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            inputDataOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            inputDataOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetRow(inputDataOptionsGrid, 2);
            inputDataGrid.Children.Add(inputDataOptionsGrid);

            List<Button> branchLengthDistributionButtons = new List<Button>();

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                branchLengthDistributionButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Sequence alignment", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                grd.Children.Add(titleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetAlignmentIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                inputDataOptionsGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedInputType = 0;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                branchLengthDistributionButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Distance matrix", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                grd.Children.Add(titleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetDistMatIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetColumn(brd, 1);

                inputDataOptionsGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedInputType = 1;
                };
            }

            Avalonia.Media.Transformation.TransformOperations.Builder opBuilder = new Avalonia.Media.Transformation.TransformOperations.Builder(1);
            opBuilder.AppendTranslate(-10, 0);
            Avalonia.Media.Transformation.TransformOperations offscreenLeft = opBuilder.Build();

            Grid sequenceAlignmentOptionsGrid = new Grid() { Margin = new Thickness(0, 5, 0, 5), Height = 109 };
            sequenceAlignmentOptionsGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            sequenceAlignmentOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            sequenceAlignmentOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            sequenceAlignmentOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            sequenceAlignmentOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            sequenceAlignmentOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            sequenceAlignmentOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            sequenceAlignmentOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            Grid.SetRow(sequenceAlignmentOptionsGrid, 1);
            Grid.SetColumnSpan(sequenceAlignmentOptionsGrid, 2);
            inputDataOptionsGrid.Children.Add(sequenceAlignmentOptionsGrid);

            sequenceAlignmentOptionsGrid.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Alignment:", VerticalAlignment = VerticalAlignment.Center });

            TextBlock inputAlignmentBlock = new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "None", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontStyle = FontStyle.Italic, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(inputAlignmentBlock, 1);
            Grid.SetColumnSpan(inputAlignmentBlock, 2);
            sequenceAlignmentOptionsGrid.Children.Add(inputAlignmentBlock);

            Dictionary<string, string> sequenceAlignment = null;

            List<string> dnaEvolutionModels = new List<string>()
            {
                "Hamming distance",
                "Jukes-Cantor",
                "Kimura 1980",
                "GTR"
            };

            List<string> proteinEvolutionModels = new List<string>()
            {
                "Hamming distance",
                "Jukes-Cantor",
                "Kimura 1984",
                "BLOSUM62"
            };

            ComboBox modelBox = new ComboBox() { Width = 180, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 2, 0, 2), VerticalAlignment = VerticalAlignment.Center, Items = dnaEvolutionModels, SelectedIndex = 3, IsEnabled = false };


            bool isProtein = false;

            {
                Button btn = new Button() { Content = "Load from file", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                Grid.SetColumn(btn, 1);
                sequenceAlignmentOptionsGrid.Children.Add(btn);

                btn.Click += async (s, e) =>
                {
                    OpenFileDialog dialog;

                    List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Alignment files", Extensions = new List<string>() { "phy", "fas", "txt" } }, new FileDialogFilter() { Name = "FASTA alignments", Extensions = new List<string>() { "fas" } }, new FileDialogFilter() { Name = "PHYLIP alignments", Extensions = new List<string>() { "phy" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

                    if (!Modules.IsMac)
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open alignment file",
                            AllowMultiple = false,
                            Filters = filters
                        };
                    }
                    else
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open alignment file",
                            AllowMultiple = false
                        };
                    }

                    string[] result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                    if (result != null && result.Length == 1)
                    {
                        isProtein = false;

                        try
                        {
                            sequenceAlignment = ReadAlignment(result[0], out isProtein);
                        }
                        catch (Exception ex)
                        {
                            await new MessageBox("Attention!", "An error occurred while reading the alignment file:\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                            return;
                        }

                        inputAlignmentBlock.Text = sequenceAlignment.Count.ToString() + " " + (isProtein ? "protein" : "DNA") + " sequences, " + sequenceAlignment.ElementAt(0).Value.Length + " characters.";

                        modelBox.IsEnabled = true;

                        if (isProtein)
                        {
                            modelBox.Items = proteinEvolutionModels;
                        }
                        else
                        {
                            modelBox.Items = dnaEvolutionModels;
                        }

                        modelBox.SelectedIndex = 3;
                    }
                };
            }

            Grid evolutionaryModelGrid = new Grid() { Margin = new Thickness(0, 5, 0, 0) };
            evolutionaryModelGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            evolutionaryModelGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            evolutionaryModelGrid.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Distance model:", VerticalAlignment = VerticalAlignment.Center });

            Grid.SetColumn(modelBox, 1);
            evolutionaryModelGrid.Children.Add(modelBox);

            Grid.SetRow(evolutionaryModelGrid, 2);
            Grid.SetColumnSpan(evolutionaryModelGrid, 3);
            sequenceAlignmentOptionsGrid.Children.Add(evolutionaryModelGrid);

            Grid bootstrapGrid = new Grid() { Margin = new Thickness(0, 5, 0, 0) };
            bootstrapGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            bootstrapGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            bootstrapGrid.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Bootstrap replicates:", VerticalAlignment = VerticalAlignment.Center });

            NumericUpDown bootstrapNud = new NumericUpDown() { Width = 100, Minimum = 0, Value = 0, FormatString = "0", VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 1, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(bootstrapNud, 1);
            bootstrapGrid.Children.Add(bootstrapNud);

            Grid.SetRow(bootstrapGrid, 3);
            Grid.SetColumnSpan(bootstrapGrid, 3);
            sequenceAlignmentOptionsGrid.Children.Add(bootstrapGrid);


            Grid distanceMatrixOptionsGrid = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            distanceMatrixOptionsGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            distanceMatrixOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            distanceMatrixOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            distanceMatrixOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            distanceMatrixOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetRow(distanceMatrixOptionsGrid, 1);
            Grid.SetColumnSpan(distanceMatrixOptionsGrid, 3);
            inputDataOptionsGrid.Children.Add(distanceMatrixOptionsGrid);

            distanceMatrixOptionsGrid.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Distance matrix:", VerticalAlignment = VerticalAlignment.Center });

            TextBlock inputDistmatBlock = new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "None", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontStyle = FontStyle.Italic, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(inputDistmatBlock, 2);
            distanceMatrixOptionsGrid.Children.Add(inputDistmatBlock);


            float[][] distanceMatrix = null;
            string[] sequenceNames = null;

            {
                Button btn = new Button() { Content = "Load from file", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                Grid.SetColumn(btn, 1);
                distanceMatrixOptionsGrid.Children.Add(btn);

                btn.Click += async (s, e) =>
                {
                    OpenFileDialog dialog;

                    List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "PHYLIP distance matrix", Extensions = new List<string>() { "phy" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

                    if (!Modules.IsMac)
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open distance matrix file",
                            AllowMultiple = false,
                            Filters = filters
                        };
                    }
                    else
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open distance matrix file",
                            AllowMultiple = false
                        };
                    }

                    string[] result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                    if (result != null && result.Length == 1)
                    {
                        try
                        {
                            distanceMatrix = ReadMatrix(result[0], out sequenceNames);

                            inputDistmatBlock.Text = distanceMatrix.Length.ToString() + " taxa";
                        }
                        catch (Exception ex)
                        {
                            await new MessageBox("Attention!", "An error occurred while reading the matrix file:\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                            return;
                        }
                    }

                };
            }

            Grid constraintRow = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            constraintRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            constraintRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            constraintRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            constraintRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            optionsPanel.Children.Add(constraintRow);

            {
                TextBlock blk = new TextBlock() { Text = "Constraint:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                constraintRow.Children.Add(blk);
            }

            TextBlock constraintInfo = new TextBlock() { Text = "None", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontStyle = FontStyle.Italic };
            Grid.SetColumn(constraintInfo, 3);
            constraintRow.Children.Add(constraintInfo);


            Button removeConstraintButton = new Button() { Content = "Remove", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0), IsEnabled = false };
            removeConstraintButton.Classes.Add("SideBarButton");
            constraintRow.Children.Add(removeConstraintButton);
            Grid.SetColumn(removeConstraintButton, 2);


            removeConstraintButton.Click += (s, e) =>
            {
                if (constraint != null)
                {
                    constraint = null;
                    constraintInfo.Text = "None";
                    removeConstraintButton.IsEnabled = false;
                }
            };

            {
                Button btn = new Button() { Content = "Load from tree", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                constraintRow.Children.Add(btn);
                Grid.SetColumn(btn, 1);

                this.AttachedToVisualTree += (s, e) => { btn.IsEnabled = this.FindAncestorOfType<MainWindow>().IsTreeOpened; this.FindAncestorOfType<MainWindow>().PropertyChanged += (s2, e2) => { if (e2.Property == MainWindow.IsTreeOpenedProperty) { btn.IsEnabled = ((MainWindow)s2).IsTreeOpened; } }; };

                btn.Click += (s, e) =>
                {
                    if (this.FindAncestorOfType<MainWindow>().IsTreeOpened)
                    {
                        constraint = this.FindAncestorOfType<MainWindow>().TransformedTree;
                        constraintInfo.Text = constraint.GetLeaves().Count().ToString() + " tips";
                        removeConstraintButton.IsEnabled = true;
                    }
                };
            }


            Grid negativeBranchLengthsContainer = new Grid() { Height = 32 };
            negativeBranchLengthsContainer.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = CheckBox.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = CheckBox.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = CheckBox.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            optionsPanel.Children.Add(negativeBranchLengthsContainer);

            CheckBox negativeBranchLengthsBox = new CheckBox() { Content = "Allow negative branch lengths", IsChecked = true, Margin = new Thickness(5, 0, 5, 0), VerticalContentAlignment = VerticalAlignment.Center, VerticalAlignment = VerticalAlignment.Top };
            negativeBranchLengthsBox.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            negativeBranchLengthsContainer.Children.Add(negativeBranchLengthsBox);

            Button createButton = new Button() { Content = "Create tree", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            createButton.Classes.Add("SideBarButton");
            Grid.SetRow(createButton, 4);
            mainContainer.Children.Add(createButton);

            this.PropertyChanged += (s, change) =>
            {
                if (change.Property == NewPage.SelectedMethodProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        modelButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        modelButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            negativeBranchLengthsContainer.IsHitTestVisible = true;
                            negativeBranchLengthsContainer.Opacity = 1;
                            negativeBranchLengthsContainer.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            negativeBranchLengthsContainer.Height = 32;
                            break;

                        case 1:
                            negativeBranchLengthsContainer.IsHitTestVisible = false;
                            negativeBranchLengthsContainer.Opacity = 0;
                            negativeBranchLengthsContainer.RenderTransform = offscreenLeft;
                            negativeBranchLengthsContainer.Height = 0;

                            break;
                    }
                }
                else if (change.Property == NewPage.SelectedInputTypeProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        branchLengthDistributionButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        branchLengthDistributionButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            sequenceAlignmentOptionsGrid.IsHitTestVisible = true;
                            distanceMatrixOptionsGrid.IsHitTestVisible = false;

                            sequenceAlignmentOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            distanceMatrixOptionsGrid.RenderTransform = offscreenLeft;

                            sequenceAlignmentOptionsGrid.Opacity = 1;
                            distanceMatrixOptionsGrid.Opacity = 0;

                            sequenceAlignmentOptionsGrid.Height = 109;
                            break;

                        case 1:
                            sequenceAlignmentOptionsGrid.IsHitTestVisible = false;
                            distanceMatrixOptionsGrid.IsHitTestVisible = true;

                            sequenceAlignmentOptionsGrid.RenderTransform = offscreenLeft;
                            distanceMatrixOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;

                            sequenceAlignmentOptionsGrid.Opacity = 0;
                            distanceMatrixOptionsGrid.Opacity = 1;

                            sequenceAlignmentOptionsGrid.Height = 0;
                            break;
                    }
                }
            };

            createButton.Click += async (s, e) =>
            {
                TreeNode tree = null;

                Func<Action<double>, TreeNode> generatorFunction = null;

                int bootstrapReplicates = (int)bootstrapNud.Value;

                if (this.SelectedMethod == 0 || this.SelectedMethod == 1)
                {
                    if (this.SelectedInputType == 0 && sequenceAlignment != null)
                    {
                        PhyloTree.TreeBuilding.AlignmentType alignmentType = isProtein ? PhyloTree.TreeBuilding.AlignmentType.Protein : PhyloTree.TreeBuilding.AlignmentType.DNA;
                        PhyloTree.TreeBuilding.EvolutionModel evolutionModel = PhyloTree.TreeBuilding.EvolutionModel.Hamming;

                        switch (modelBox.SelectedIndex)
                        {
                            case 0:
                                evolutionModel = PhyloTree.TreeBuilding.EvolutionModel.Hamming;
                                break;

                            case 1:
                                evolutionModel = PhyloTree.TreeBuilding.EvolutionModel.JukesCantor;
                                break;

                            case 2:
                                evolutionModel = PhyloTree.TreeBuilding.EvolutionModel.Kimura;
                                break;

                            case 3:
                                evolutionModel = isProtein ? PhyloTree.TreeBuilding.EvolutionModel.BLOSUM62 : PhyloTree.TreeBuilding.EvolutionModel.GTR;
                                break;
                        }

                        bool allowNegativeBranches = negativeBranchLengthsBox.IsChecked == true;

                        if (this.SelectedMethod == 0)
                        {
                            generatorFunction = x => PhyloTree.TreeBuilding.NeighborJoining.BuildTree(sequenceAlignment, evolutionModel, bootstrapReplicates, alignmentType, constraint, allowNegativeBranches, progressCallback: x);
                        }
                        else if (this.SelectedMethod == 1)
                        {
                            generatorFunction = x => PhyloTree.TreeBuilding.UPGMA.BuildTree(sequenceAlignment, evolutionModel, bootstrapReplicates, alignmentType, constraint, progressCallback: x);
                        }
                    }
                    else if (this.SelectedInputType == 1 && distanceMatrix != null)
                    {
                        bool allowNegativeBranches = negativeBranchLengthsBox.IsChecked == true;

                        if (this.SelectedMethod == 0)
                        {
                            generatorFunction = x => PhyloTree.TreeBuilding.NeighborJoining.BuildTree(distanceMatrix, sequenceNames, constraint, copyMatrix: true, allowNegativeBranches, progressCallback: x);
                        }
                        else if (this.SelectedMethod == 1)
                        {
                            generatorFunction = x => PhyloTree.TreeBuilding.UPGMA.BuildTree(distanceMatrix, sequenceNames, constraint, copyMatrix: true, progressCallback: x);
                        }
                    }
                }

                if (generatorFunction != null)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        ProgressWindow progWin = new ProgressWindow() { ProgressText = "Building tree...", IsIndeterminate = false };

                        if (this.SelectedInputType == 0 && bootstrapReplicates == 0)
                        {
                            progWin.Steps = 2;
                        }

                        Task task = progWin.ShowDialog2(this.FindAncestorOfType<Window>());

                        await Task.Run(async () =>
                        {
                            tree = generatorFunction(async x =>
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    if (this.SelectedInputType == 0 && bootstrapReplicates == 0)
                                    {
                                        progWin.LabelText = x <= 0.5 ? "Building distance matrix..." : "Building tree...";
                                    }

                                    progWin.Progress = x;
                                });
                            });

                            if (constraint != null)
                            {
                                SetConstraint(tree, constraint);
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progWin.Close();
                            });
                        });

                        await task;
                    });
                }

                if (tree != null)
                {
                    string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                    if (constraint != null)
                    {
                        using (System.IO.StreamReader constraintModulesReader = GetConstraintModules(this.SelectedMethod == 1))
                        {
                            PhyloTree.Formats.NEXUS.WriteAllTrees(new TreeNode[] { tree }, tempFile, additionalNexusBlocks: constraintModulesReader);
                        }
                    }
                    else
                    {
                        PhyloTree.Formats.NEXUS.WriteAllTrees(new TreeNode[] { tree }, tempFile);
                    }

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await this.FindAncestorOfType<MainWindow>().LoadFile(tempFile, true); });
                }
            };

            return mainContainer;
        }

        private Control GetRandomPage()
        {
            Grid mainContainer = new Grid() { Margin = new Avalonia.Thickness(25, 0, 0, 0) };
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star) { MaxHeight = 320 });
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            mainContainer.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Random tree" });

            StackPanel descriptionPanel = new StackPanel();


            descriptionPanel.Children.Add(new TextBlock()
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Text = "Sample a random tree using various different models. Models sampling a cladogram produce a topology with random branch lengths; models sampling a chronogram produce a clock-like tree with branch lengths in units of time.",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            });


            Grid.SetRow(descriptionPanel, 1);
            mainContainer.Children.Add(descriptionPanel);

            Grid modelGrid = new Grid();
            modelGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            modelGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            modelGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            modelGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            modelGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            {
                TextBlock blk = new TextBlock() { Text = "Cladogram models", FontWeight = FontWeight.Bold, FontSize = 14, Margin = new Thickness(5, 0, 5, 0), HorizontalAlignment = HorizontalAlignment.Center };
                modelGrid.Children.Add(blk);
            }

            {
                TextBlock blk = new TextBlock() { Text = "Chronogram models", FontWeight = FontWeight.Bold, FontSize = 14, Margin = new Thickness(5, 0, 5, 0), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetColumn(blk, 1);
                modelGrid.Children.Add(blk);
            }

            TextBlock constraintWarning = new TextBlock() { Text = "Note: using a constraint will bias the topology distribution!", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontStyle = FontStyle.Italic, FontSize = 12, TextWrapping = TextWrapping.Wrap };
            Grid.SetRow(constraintWarning, 2);
            Grid.SetColumnSpan(constraintWarning, 4);

            TreeNode constraint = null;

            List<Button> modelButtons = new List<Button>();

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "PDA model", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "All tree topologies have equal probability", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetPDAIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetRow(brd, 1);

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedModel = 0;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "YHK model", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "Pure birth process", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetYHKIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetRow(brd, 2);

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedModel = 1;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Coalescent model", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "Produces top-heavy trees", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetCoalIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetRow(brd, 1);
                Grid.SetColumn(brd, 1);

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedModel = 2;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 80, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                modelButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Birth-death model", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = "With the option to include extinct lineages", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetBDIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetRow(brd, 2);
                Grid.SetColumn(brd, 1);

                modelGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedModel = 3;
                };
            }

            Grid.SetRow(modelGrid, 2);
            mainContainer.Children.Add(modelGrid);

            ScrollViewer optionsScroller = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, AllowAutoHide = false, Padding = new Thickness(0, 0, 18, 0), Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(optionsScroller, 3);
            mainContainer.Children.Add(optionsScroller);

            StackPanel optionsPanel = new StackPanel();
            optionsScroller.Content = optionsPanel;

            Grid commonOptions = new Grid() { Margin = new Thickness(0, 0, 0, 0) };
            commonOptions.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            commonOptions.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            commonOptions.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            Grid commonOptionsZerothRow = new Grid();
            commonOptionsZerothRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsZerothRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            commonOptionsZerothRow.Children.Add(new TextBlock() { Text = "Number of trees to create:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });

            NumericUpDown numberOfTrees = new NumericUpDown() { Minimum = 1, Value = 1, FormatString = "0", VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 1, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(numberOfTrees, 1);
            commonOptionsZerothRow.Children.Add(numberOfTrees);

            commonOptions.Children.Add(commonOptionsZerothRow);

            Grid commonOptionsFirstRow = new Grid();
            Grid.SetRow(commonOptionsFirstRow, 1);
            commonOptionsFirstRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            commonOptionsFirstRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            CheckBox labelledTree = new CheckBox() { Content = "Labelled tree", IsChecked = true, Margin = new Thickness(5, 0, 5, 0), VerticalContentAlignment = VerticalAlignment.Center };
            commonOptionsFirstRow.Children.Add(labelledTree);

            Grid numberOfLeavesGrid = new Grid();
            numberOfLeavesGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            numberOfLeavesGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(numberOfLeavesGrid, 1);
            commonOptionsFirstRow.Children.Add(numberOfLeavesGrid);

            string[] leafNames = new string[10];

            for (int i = 0; i < 10; i++)
            {
                leafNames[i] = "t" + (i + 1).ToString();
            }

            numberOfLeavesGrid.Children.Add(new TextBlock() { Text = "Number of tips:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });

            bool suppressChange = false;

            NumericUpDown numberOfLeaves = new NumericUpDown() { Minimum = 2, Value = 10, FormatString = "0", VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 1, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(numberOfLeaves, 1);
            numberOfLeavesGrid.Children.Add(numberOfLeaves);

            commonOptions.Children.Add(commonOptionsFirstRow);

            Grid commonOptionsSecondRow = new Grid() { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 0), Height = 85, ClipToBounds = true };
            commonOptionsSecondRow.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            commonOptionsSecondRow.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            commonOptionsSecondRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            commonOptionsSecondRow.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            Grid.SetRow(commonOptionsSecondRow, 2);
            commonOptions.Children.Add(commonOptionsSecondRow);

            commonOptionsSecondRow.Children.Add(new TextBlock() { Text = "Labels:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });

            {
                Button btn = new Button() { Content = "From current tree", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                commonOptionsSecondRow.Children.Add(btn);

                Grid.SetColumn(btn, 1);

                this.AttachedToVisualTree += (s, e) => { btn.IsEnabled = this.FindAncestorOfType<MainWindow>().IsTreeOpened; this.FindAncestorOfType<MainWindow>().PropertyChanged += (s2, e2) => { if (e2.Property == MainWindow.IsTreeOpenedProperty) { btn.IsEnabled = ((MainWindow)s2).IsTreeOpened; } }; };

                btn.Click += async (s, e) =>
                {
                    if (this.FindAncestorOfType<MainWindow>().IsTreeOpened)
                    {
                        string[] newLeaves = this.FindAncestorOfType<MainWindow>().TransformedTree.GetLeafNames().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        if (newLeaves.Length >= 2)
                        {
                            leafNames = newLeaves;

                            suppressChange = true;
                            numberOfLeaves.Value = leafNames.Length;
                            suppressChange = false;
                        }
                        else
                        {
                            await new MessageBox("Attention", "A tree needs to have at least two tips!").ShowDialog2(this.FindAncestorOfType<Window>());
                        }
                    }
                };
            }

            {
                Button btn = new Button() { Content = "From file", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                Grid.SetColumn(btn, 2);
                commonOptionsSecondRow.Children.Add(btn);

                btn.Click += async (s, e) =>
                {
                    OpenFileDialog dialog;

                    List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Text files", Extensions = new List<string>() { "csv", "txt" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

                    if (!Modules.IsMac)
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open text file",
                            AllowMultiple = false,
                            Filters = filters
                        };
                    }
                    else
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Open text file",
                            AllowMultiple = false
                        };
                    }

                    string[] result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                    if (result != null && result.Length == 1)
                    {
                        string[] newLeaves = System.IO.File.ReadAllLines(result[0]).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                        if (newLeaves.Length >= 2)
                        {
                            leafNames = newLeaves;

                            suppressChange = true;
                            numberOfLeaves.Value = leafNames.Length;
                            suppressChange = false;
                        }
                        else
                        {
                            await new MessageBox("Attention", "A tree needs to have at least two tips!").ShowDialog2(this.FindAncestorOfType<Window>());
                        }
                    }
                };
            }

            {
                Button btn = new Button() { Content = "View/edit", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                Grid.SetColumn(btn, 3);
                commonOptionsSecondRow.Children.Add(btn);

                btn.Click += async (s, e) =>
                {
                    TextEditorWindow window = new TextEditorWindow();
                    window.Text = string.Join("\n", leafNames);
                    await window.ShowDialog2(this.FindAncestorOfType<Window>());

                    if (window.Result)
                    {
                        string[] newLeaves = window.Text.Replace('\r', '\n').Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                        if (newLeaves.Length >= 2)
                        {
                            leafNames = newLeaves;

                            suppressChange = true;
                            numberOfLeaves.Value = leafNames.Length;
                            suppressChange = false;
                        }
                        else
                        {
                            await new MessageBox("Attention", "A tree needs to have at least two tips!").ShowDialog2(this.FindAncestorOfType<Window>());
                        }
                    }
                };
            }

            Grid commonOptionsThirdRow = new Grid() { Margin = new Thickness(0, 5, 0, 0) };
            commonOptionsThirdRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsThirdRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsThirdRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            commonOptionsThirdRow.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetRow(commonOptionsThirdRow, 1);
            Grid.SetColumnSpan(commonOptionsThirdRow, 4);
            commonOptionsSecondRow.Children.Add(commonOptionsThirdRow);

            {
                TextBlock blk = new TextBlock() { Text = "Constraint:", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                commonOptionsThirdRow.Children.Add(blk);
            }

            TextBlock constraintInfo = new TextBlock() { Text = "None", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontStyle = FontStyle.Italic };
            Grid.SetColumn(constraintInfo, 3);
            commonOptionsThirdRow.Children.Add(constraintInfo);


            Button removeConstraintButton = new Button() { Content = "Remove", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0), IsEnabled = false };
            removeConstraintButton.Classes.Add("SideBarButton");
            commonOptionsThirdRow.Children.Add(removeConstraintButton);
            Grid.SetColumn(removeConstraintButton, 2);


            removeConstraintButton.Click += (s, e) =>
            {
                if (constraint != null)
                {
                    constraint = null;
                    constraintInfo.Text = "None";
                    removeConstraintButton.IsEnabled = false;
                    constraintWarning.IsVisible = false;
                }
            };

            {
                Button btn = new Button() { Content = "Load from tree", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
                btn.Classes.Add("SideBarButton");
                commonOptionsThirdRow.Children.Add(btn);
                Grid.SetColumn(btn, 1);

                this.AttachedToVisualTree += (s, e) => { btn.IsEnabled = this.FindAncestorOfType<MainWindow>().IsTreeOpened; this.FindAncestorOfType<MainWindow>().PropertyChanged += (s2, e2) => { if (e2.Property == MainWindow.IsTreeOpenedProperty) { btn.IsEnabled = ((MainWindow)s2).IsTreeOpened; } }; };

                btn.Click += (s, e) =>
                {
                    if (this.FindAncestorOfType<MainWindow>().IsTreeOpened)
                    {
                        constraint = this.FindAncestorOfType<MainWindow>().TransformedTree;
                        constraintInfo.Text = constraint.GetLeaves().Count().ToString() + " tips";
                        removeConstraintButton.IsEnabled = true;
                        constraintWarning.IsVisible = this.SelectedModel != 0;
                    }
                };
            }

            commonOptionsSecondRow.Children.Add(constraintWarning);

            //Grid.SetRow(commonOptions, 3);
            //mainContainer.Children.Add(commonOptions);
            optionsPanel.Children.Add(commonOptions);

            Grid branchLengthDistributionGridContainer = new Grid() { Opacity = 1, RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity, Height = 149, VerticalAlignment = VerticalAlignment.Top };
            branchLengthDistributionGridContainer.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };

            Grid branchLengthDistributionGrid = new Grid() { VerticalAlignment = VerticalAlignment.Top };
            branchLengthDistributionGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            branchLengthDistributionGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            branchLengthDistributionGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            CheckBox rootedTree = new CheckBox() { Content = "Rooted", IsChecked = false, Margin = new Thickness(5, 0, 5, 0), VerticalContentAlignment = VerticalAlignment.Center };
            branchLengthDistributionGrid.Children.Add(rootedTree);

            CheckBox branchLengths = new CheckBox() { Content = "Branch lengths", IsChecked = true, Margin = new Thickness(5, 0, 5, 0), VerticalContentAlignment = VerticalAlignment.Center, VerticalAlignment = VerticalAlignment.Top };
            Grid.SetRow(branchLengths, 1);
            branchLengthDistributionGrid.Children.Add(branchLengths);

            Grid branchLengthDistributionOptionsGrid = new Grid() { Height = 85 };
            branchLengthDistributionOptionsGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            branchLengthDistributionOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            branchLengthDistributionOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            branchLengthDistributionOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            branchLengthDistributionOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            branchLengthDistributionOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            branchLengthDistributionOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetRow(branchLengthDistributionOptionsGrid, 2);
            branchLengthDistributionGrid.Children.Add(branchLengthDistributionOptionsGrid);

            List<Button> branchLengthDistributionButtons = new List<Button>();

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                branchLengthDistributionButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Uniform", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                grd.Children.Add(titleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetUniformIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                branchLengthDistributionOptionsGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedDistribution = 0;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                branchLengthDistributionButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Gamma", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                grd.Children.Add(titleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetGammaIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetColumn(brd, 1);

                branchLengthDistributionOptionsGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedDistribution = 1;
                };
            }

            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");

                branchLengthDistributionButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = "Exponential", Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                grd.Children.Add(titleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(GetExponentialIcon) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                Grid.SetColumn(brd, 2);

                branchLengthDistributionOptionsGrid.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedDistribution = 2;
                };
            }

            Avalonia.Media.Transformation.TransformOperations.Builder opBuilder = new Avalonia.Media.Transformation.TransformOperations.Builder(1);
            opBuilder.AppendTranslate(-10, 0);
            Avalonia.Media.Transformation.TransformOperations offscreenLeft = opBuilder.Build();

            Grid uniformDistributionOptions = new Grid() { Margin = new Thickness(0, 5, 0, 5), VerticalAlignment = VerticalAlignment.Top };
            uniformDistributionOptions.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            uniformDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            uniformDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            uniformDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            uniformDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            uniformDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetRow(uniformDistributionOptions, 1);
            Grid.SetColumnSpan(uniformDistributionOptions, 3);
            branchLengthDistributionOptionsGrid.Children.Add(uniformDistributionOptions);

            uniformDistributionOptions.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Minimum:", VerticalAlignment = VerticalAlignment.Center });

            {
                TextBlock blk = new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "Maximum:", VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                uniformDistributionOptions.Children.Add(blk);
            }

            NumericUpDown uniformMinNUD = new NumericUpDown() { Minimum = 0, Value = 0, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(uniformMinNUD, 1);
            uniformDistributionOptions.Children.Add(uniformMinNUD);

            NumericUpDown uniformMaxNUD = new NumericUpDown() { Minimum = 0, Value = 10, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(uniformMaxNUD, 3);
            uniformDistributionOptions.Children.Add(uniformMaxNUD);


            Grid gammaDistributionOptions = new Grid() { Margin = new Thickness(0, 5, 0, 5), VerticalAlignment = VerticalAlignment.Top };
            gammaDistributionOptions.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            gammaDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            gammaDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            gammaDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            gammaDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            gammaDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetRow(gammaDistributionOptions, 1);
            Grid.SetColumnSpan(gammaDistributionOptions, 3);
            branchLengthDistributionOptionsGrid.Children.Add(gammaDistributionOptions);

            gammaDistributionOptions.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "α:", VerticalAlignment = VerticalAlignment.Center });

            {
                TextBlock blk = new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "β:", VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                gammaDistributionOptions.Children.Add(blk);
            }

            NumericUpDown gammaANud = new NumericUpDown() { Minimum = double.Epsilon, Value = 2, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(gammaANud, 1);
            gammaDistributionOptions.Children.Add(gammaANud);

            NumericUpDown gammaBNud = new NumericUpDown() { Minimum = double.Epsilon, Value = 1, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(gammaBNud, 3);
            gammaDistributionOptions.Children.Add(gammaBNud);

            Grid exponentialDistributionOptions = new Grid() { Margin = new Thickness(0, 5, 0, 5), VerticalAlignment = VerticalAlignment.Top };
            exponentialDistributionOptions.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            exponentialDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
            exponentialDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            exponentialDistributionOptions.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetRow(exponentialDistributionOptions, 1);
            Grid.SetColumnSpan(exponentialDistributionOptions, 3);
            branchLengthDistributionOptionsGrid.Children.Add(exponentialDistributionOptions);

            exponentialDistributionOptions.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = "λ:", VerticalAlignment = VerticalAlignment.Center });

            NumericUpDown exponentialLNud = new NumericUpDown() { Minimum = double.Epsilon, Value = 0.5, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(exponentialLNud, 1);
            exponentialDistributionOptions.Children.Add(exponentialLNud);

            Grid alternativeOptionsGrid = new Grid();
            alternativeOptionsGrid.Children.Add(branchLengthDistributionGridContainer);
            branchLengthDistributionGridContainer.Children.Add(branchLengthDistributionGrid);

            optionsPanel.Children.Add(alternativeOptionsGrid);

            Grid birthDeathContainerGrid = new Grid() { Opacity = 0, RenderTransform = offscreenLeft, IsHitTestVisible = false, VerticalAlignment = VerticalAlignment.Top, Height = 0 };
            birthDeathContainerGrid.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.DoubleTransition() { Property = Grid.HeightProperty, Duration = TimeSpan.FromMilliseconds(100) }, new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
            alternativeOptionsGrid.Children.Add(birthDeathContainerGrid);

            Grid birthDeathOptionsGrid = new Grid() { VerticalAlignment = VerticalAlignment.Top };
            birthDeathOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            birthDeathOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            birthDeathOptionsGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            birthDeathOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            birthDeathOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            birthDeathContainerGrid.Children.Add(birthDeathOptionsGrid);

            {
                TextBlock blk = new TextBlock() { Text = "Birth rate:", Margin = new Thickness(5, 5, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                birthDeathOptionsGrid.Children.Add(blk);
            }

            {
                TextBlock blk = new TextBlock() { Text = "Death rate:", Margin = new Thickness(5, 5, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                Grid.SetRow(blk, 1);
                birthDeathOptionsGrid.Children.Add(blk);
            }

            NumericUpDown birthRateNud = new NumericUpDown() { Minimum = double.Epsilon, Value = 0.5, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetColumn(birthRateNud, 1);
            birthDeathOptionsGrid.Children.Add(birthRateNud);

            NumericUpDown deathRateNud = new NumericUpDown() { Minimum = 0, Value = 0, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 5, 0), Padding = new Avalonia.Thickness(2, 0, 2, 0), Increment = 0.1, VerticalAlignment = VerticalAlignment.Center, Width = 100 };
            Grid.SetRow(deathRateNud, 1);
            Grid.SetColumn(deathRateNud, 1);
            birthDeathOptionsGrid.Children.Add(deathRateNud);

            CheckBox keepDead = new CheckBox() { Content = "Keep extinct lineages", IsChecked = false, Margin = new Thickness(5, 5, 5, 0), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetRow(keepDead, 2);
            Grid.SetColumnSpan(keepDead, 2);
            birthDeathOptionsGrid.Children.Add(keepDead);



            Button createButton = new Button() { Content = "Create tree", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            createButton.Classes.Add("SideBarButton");
            Grid.SetRow(createButton, 4);
            mainContainer.Children.Add(createButton);

            this.PropertyChanged += (s, change) =>
            {
                if (change.Property == NewPage.SelectedModelProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        modelButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        modelButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            constraintWarning.IsVisible = false;

                            branchLengthDistributionGridContainer.IsHitTestVisible = true;
                            branchLengthDistributionGridContainer.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            branchLengthDistributionGridContainer.Opacity = 1;
                            branchLengthDistributionGridContainer.Height = branchLengths.IsChecked == true ? 149 : 64;

                            birthDeathContainerGrid.IsHitTestVisible = false;
                            birthDeathContainerGrid.RenderTransform = offscreenLeft;
                            birthDeathContainerGrid.Opacity = 0;
                            birthDeathContainerGrid.Height = 0;
                            break;

                        case 1:
                            constraintWarning.IsVisible = constraint != null;

                            branchLengthDistributionGridContainer.IsHitTestVisible = true;
                            branchLengthDistributionGridContainer.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            branchLengthDistributionGridContainer.Opacity = 1;
                            branchLengthDistributionGridContainer.Height = branchLengths.IsChecked == true ? 149 : 64;

                            birthDeathContainerGrid.IsHitTestVisible = false;
                            birthDeathContainerGrid.RenderTransform = offscreenLeft;
                            birthDeathContainerGrid.Opacity = 0;
                            birthDeathContainerGrid.Height = 0;
                            break;

                        case 2:
                            constraintWarning.IsVisible = constraint != null;

                            branchLengthDistributionGridContainer.IsHitTestVisible = false;
                            branchLengthDistributionGridContainer.RenderTransform = offscreenLeft;
                            branchLengthDistributionGridContainer.Opacity = 0;
                            branchLengthDistributionGridContainer.Height = 0;

                            birthDeathContainerGrid.IsHitTestVisible = false;
                            birthDeathContainerGrid.RenderTransform = offscreenLeft;
                            birthDeathContainerGrid.Opacity = 0;
                            birthDeathContainerGrid.Height = 0;
                            break;

                        case 3:
                            constraintWarning.IsVisible = constraint != null;

                            branchLengthDistributionGridContainer.IsHitTestVisible = false;
                            branchLengthDistributionGridContainer.RenderTransform = offscreenLeft;
                            branchLengthDistributionGridContainer.Opacity = 0;
                            branchLengthDistributionGridContainer.Height = 0;

                            birthDeathContainerGrid.IsHitTestVisible = true;
                            birthDeathContainerGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            birthDeathContainerGrid.Opacity = 1;
                            birthDeathContainerGrid.Height = 93;
                            break;
                    }
                }
                else if (change.Property == NewPage.SelectedDistributionProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        branchLengthDistributionButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        branchLengthDistributionButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            uniformDistributionOptions.IsHitTestVisible = true;
                            gammaDistributionOptions.IsHitTestVisible = false;
                            exponentialDistributionOptions.IsHitTestVisible = false;

                            uniformDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            gammaDistributionOptions.RenderTransform = offscreenLeft;
                            exponentialDistributionOptions.RenderTransform = offscreenLeft;

                            uniformDistributionOptions.Opacity = 1;
                            gammaDistributionOptions.Opacity = 0;
                            exponentialDistributionOptions.Opacity = 0;
                            break;

                        case 1:
                            uniformDistributionOptions.IsHitTestVisible = false;
                            gammaDistributionOptions.IsHitTestVisible = true;
                            exponentialDistributionOptions.IsHitTestVisible = false;

                            uniformDistributionOptions.RenderTransform = offscreenLeft;
                            gammaDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                            exponentialDistributionOptions.RenderTransform = offscreenLeft;

                            uniformDistributionOptions.Opacity = 0;
                            gammaDistributionOptions.Opacity = 1;
                            exponentialDistributionOptions.Opacity = 0;
                            break;

                        case 2:
                            uniformDistributionOptions.IsHitTestVisible = false;
                            gammaDistributionOptions.IsHitTestVisible = false;
                            exponentialDistributionOptions.IsHitTestVisible = true;

                            uniformDistributionOptions.RenderTransform = offscreenLeft;
                            gammaDistributionOptions.RenderTransform = offscreenLeft;
                            exponentialDistributionOptions.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;

                            uniformDistributionOptions.Opacity = 0;
                            gammaDistributionOptions.Opacity = 0;
                            exponentialDistributionOptions.Opacity = 1;
                            break;
                    }
                }
            };

            numberOfLeaves.ValueChanged += (s, e) =>
            {
                if (!suppressChange)
                {
                    int newN = (int)numberOfLeaves.Value;
                    leafNames = new string[newN];

                    for (int i = 0; i < leafNames.Length; i++)
                    {
                        leafNames[i] = "t" + (i + 1).ToString();
                    }
                }
            };

            labelledTree.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    constraint = null;
                    constraintWarning.IsVisible = false;

                    if (labelledTree.IsChecked == true)
                    {
                        commonOptionsSecondRow.IsHitTestVisible = true;
                        commonOptionsSecondRow.Opacity = 1;
                        commonOptionsSecondRow.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                        commonOptionsSecondRow.Height = 85;
                    }
                    else
                    {
                        commonOptionsSecondRow.IsHitTestVisible = false;
                        commonOptionsSecondRow.Opacity = 0;
                        commonOptionsSecondRow.RenderTransform = offscreenLeft;
                        commonOptionsSecondRow.Height = 0;
                    }
                }
            };

            branchLengths.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    if (branchLengths.IsChecked == true)
                    {
                        branchLengthDistributionOptionsGrid.IsHitTestVisible = true;
                        branchLengthDistributionOptionsGrid.Opacity = 1;
                        branchLengthDistributionOptionsGrid.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                        branchLengthDistributionOptionsGrid.Height = 85;
                        branchLengthDistributionGridContainer.Height = 149;
                    }
                    else
                    {
                        branchLengthDistributionOptionsGrid.IsHitTestVisible = false;
                        branchLengthDistributionOptionsGrid.Opacity = 0;
                        branchLengthDistributionOptionsGrid.RenderTransform = offscreenLeft;
                        branchLengthDistributionOptionsGrid.Height = 0;
                        branchLengthDistributionGridContainer.Height = 64;
                    }
                }
            };

            createButton.Click += async (s, e) =>
            {
                TreeNode[] trees = null;

                Func<TreeNode> generatorFunction = null;

                System.Threading.CancellationTokenSource cancellationTokenSource = null;
                System.Threading.CancellationToken cancellationToken = default;

                bool isLabelled = labelledTree.IsChecked == true;
                bool rooted = rootedTree.IsChecked == true || (int)numberOfLeaves.Value == 2 || this.SelectedModel >= 2;

                if (this.SelectedModel == 0 || this.SelectedModel == 1)
                {
                    TreeNode.NullHypothesis model = this.SelectedModel == 0 ? TreeNode.NullHypothesis.PDA : TreeNode.NullHypothesis.YHK;

                    if (labelledTree.IsChecked == true)
                    {
                        if (branchLengths.IsChecked == true)
                        {
                            IContinuousDistribution distribution = new ContinuousUniform(0, 10);

                            if (SelectedDistribution == 0)
                            {
                                distribution = new ContinuousUniform(Math.Min(uniformMinNUD.Value, uniformMaxNUD.Value), Math.Max(uniformMinNUD.Value, uniformMaxNUD.Value), PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }
                            else if (SelectedDistribution == 1)
                            {
                                distribution = new Gamma(gammaANud.Value, gammaBNud.Value, PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }
                            else if (SelectedDistribution == 2)
                            {
                                distribution = new Exponential(exponentialLNud.Value, PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }

                            generatorFunction = () => PhyloTree.TreeBuilding.RandomTree.LabelledTree(leafNames, distribution, model, rooted, constraint: constraint);
                        }
                        else
                        {
                            generatorFunction = () => PhyloTree.TreeBuilding.RandomTree.LabelledTopology(leafNames, model, rooted, constraint: constraint);
                        }
                    }
                    else
                    {
                        int numLeaves = (int)numberOfLeaves.Value;
                        if (branchLengths.IsChecked == true)
                        {
                            IContinuousDistribution distribution = new ContinuousUniform(0, 10);

                            if (SelectedDistribution == 0)
                            {
                                distribution = new ContinuousUniform(Math.Min(uniformMinNUD.Value, uniformMaxNUD.Value), Math.Max(uniformMinNUD.Value, uniformMaxNUD.Value), PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }
                            else if (SelectedDistribution == 1)
                            {
                                distribution = new Gamma(gammaANud.Value, gammaBNud.Value, PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }
                            else if (SelectedDistribution == 2)
                            {
                                distribution = new Exponential(exponentialLNud.Value, PhyloTree.TreeBuilding.RandomTree.RandomNumberGenerator);
                            }

                            generatorFunction = () => PhyloTree.TreeBuilding.RandomTree.UnlabelledTree(numLeaves, distribution, model, rooted);
                        }
                        else
                        {
                            generatorFunction = () => PhyloTree.TreeBuilding.RandomTree.UnlabelledTopology(numLeaves, model, rooted);
                        }
                    }
                }
                else if (this.SelectedModel == 2)
                {
                    if (labelledTree.IsChecked == true)
                    {
                        generatorFunction = () => PhyloTree.TreeBuilding.CoalescentTree.LabelledTree(leafNames, constraint);
                    }
                    else
                    {
                        int numLeaves = (int)numberOfLeaves.Value;
                        generatorFunction = () => PhyloTree.TreeBuilding.CoalescentTree.UnlabelledTree(numLeaves);
                    }
                }
                else if (this.SelectedModel == 3)
                {
                    cancellationTokenSource = new System.Threading.CancellationTokenSource();
                    cancellationToken = cancellationTokenSource.Token;

                    if (labelledTree.IsChecked == true)
                    {
                        double birthRate = birthRateNud.Value;
                        double deathRate = deathRateNud.Value;
                        bool keepDeadLineages = keepDead.IsChecked == true;

                        generatorFunction = () => PhyloTree.TreeBuilding.BirthDeathTree.LabelledTree(leafNames, birthRate, deathRate, keepDeadLineages, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        int numLeaves = (int)numberOfLeaves.Value;
                        double birthRate = birthRateNud.Value;
                        double deathRate = deathRateNud.Value;
                        bool keepDeadLineages = keepDead.IsChecked == true;

                        generatorFunction = () => PhyloTree.TreeBuilding.BirthDeathTree.UnlabelledTree(numLeaves, birthRate, deathRate, keepDeadLineages, cancellationToken: cancellationToken);
                    }
                }

                if (generatorFunction != null)
                {
                    int countTrees = (int)numberOfTrees.Value;

                    trees = new TreeNode[countTrees];

                    bool wasCancelled = false;

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        ProgressWindow progWin = new ProgressWindow() { ProgressText = "Sampling trees...", IsIndeterminate = false, CancellationTokenSource = cancellationTokenSource };

                        Task task = progWin.ShowDialog2(this.FindAncestorOfType<Window>());

                        object progressLock = new object();
                        int progress = 0;

                        await Task.Run(async () =>
                        {
                            Parallel.For(0, countTrees, (i, state) =>
                            {
                                try
                                {
                                    trees[i] = generatorFunction();

                                    if (constraint != null && isLabelled == true)
                                    {
                                        SetConstraint(trees[i], constraint);
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    lock (progressLock)
                                    {
                                        wasCancelled = true;
                                    }

                                    state.Stop();
                                }

                                lock (progressLock)
                                {
                                    progress++;
                                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        progWin.Progress = (double)progress / countTrees;
                                    });
                                }
                            });

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progWin.Close();
                            });
                        });

                        await task;
                    });

                    cancellationTokenSource?.Dispose();

                    if (wasCancelled)
                    {
                        return;
                    }
                }

                if (trees != null)
                {
                    string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                    if (constraint != null && isLabelled)
                    {
                        using (System.IO.StreamReader constraintModulesReader = GetConstraintModules(rooted))
                        {
                            PhyloTree.Formats.NEXUS.WriteAllTrees(trees, tempFile, additionalNexusBlocks: constraintModulesReader);
                        }
                    }
                    else
                    {
                        PhyloTree.Formats.NEXUS.WriteAllTrees(trees, tempFile);
                    }

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await this.FindAncestorOfType<MainWindow>().LoadFile(tempFile, true); });
                }
            };

            return mainContainer;
        }

        private static string GetSerializedModules(bool rooted)
        {
            List<List<string[]>> allModules = new List<List<string[]>>();

            List<string[]> transformerModule = new List<string[]>();

            transformerModule.Add(new string[] { "32914d41-b182-461e-b7c6-5f0263cc1ccd", MainWindow.SerializeParameters(new Dictionary<string, object>()) });

            allModules.Add(transformerModule);

            List<string[]> furtherTransformationModules = new List<string[]>();

            allModules.Add(furtherTransformationModules);

            if (rooted)
            {
                // Rectangular coordinates
                List<string[]> coordinatesModule = new List<string[]>() { new string[] { "68e25ec6-5911-4741-8547-317597e1b792", MainWindow.SerializeParameters(new Dictionary<string, object>()) } };
                allModules.Add(coordinatesModule);
            }
            else
            {
                // Radial coordinates
                List<string[]> coordinatesModule = new List<string[]>() { new string[] { "95b61284-b870-48b9-b51c-3276f7d89df1", MainWindow.SerializeParameters(new Dictionary<string, object>()) } };
                allModules.Add(coordinatesModule);
            }

            List<string[]> plottingActionModules = new List<string[]>();
            plottingActionModules.Add(new string[] { "@Background", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Colour", Colour.FromRgb(255, 255, 255) } }) });


            NumberFormatterOptions formatterOptions = new NumberFormatterOptions(Modules.DefaultAttributeConvertersToDouble[1], true);
            formatterOptions.AttributeName = "ConstraintThickness";
            formatterOptions.AttributeType = "Number";
            formatterOptions.DefaultValue = 0;
            formatterOptions.Parameters = new object[] {
                    Modules.DefaultAttributeConvertersToDouble[1],
                    0.0,
                    double.PositiveInfinity,
                    true
                };

            ColourFormatterOptions branchColour = new ColourFormatterOptions(Modules.DefaultAttributeConvertersToColour[0], new object[] {
                    Modules.DefaultAttributeConvertersToColour[0],
                    0,
                    1,
                    Modules.DefaultGradients["TransparentToBlack"],
                    true
                    })
            { AttributeName = "Color", AttributeType = "String", DefaultColour = Colour.FromRgb(86, 180, 233) };


            if (rooted)
            {
                plottingActionModules.Add(new string[] { "7c767b07-71be-48b2-8753-b27f3e973570", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Line weight:", formatterOptions }, { "Colour:", branchColour }, { "Shape:", 0.0 } }) });
                plottingActionModules.Add(new string[] { "7c767b07-71be-48b2-8753-b27f3e973570", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Shape:", 0.0 } } ) });
                plottingActionModules.Add(new string[] { "ac496677-2650-4d92-8646-0812918bab03", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Branch reference:", 0 } }) });
                plottingActionModules.Add(new string[] { "ac496677-2650-4d92-8646-0812918bab03", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Show on:", 2 }, { "Anchor:", 1 }, { "Position:", new VectSharp.Point(0, -5) }, { "Font:", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily("Helvetica"), 8) }, { "Attribute:", "Length" }, { "Attribute type:", "Number" }, { "Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } } } }) });
            }
            else
            {
                plottingActionModules.Add(new string[] { "7c767b07-71be-48b2-8753-b27f3e973570", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Root branch", false }, { "Shape:", 1.0 }, { "Line weight:", formatterOptions }, { "Colour:", branchColour } }) });
                plottingActionModules.Add(new string[] { "7c767b07-71be-48b2-8753-b27f3e973570", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Root branch", false }, { "Shape:", 1.0 } }) });
                plottingActionModules.Add(new string[] { "ac496677-2650-4d92-8646-0812918bab03", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Reference:", 1 }, { "Branch reference:", 1 } }) });
            }

            allModules.Add(plottingActionModules);


            string serializedModules = System.Text.Json.JsonSerializer.Serialize(allModules, Modules.DefaultSerializationOptions);

            string signature = CryptoUtils.SignString(serializedModules, CryptoUtils.FileRSAEncrypter);

            string publicKeySerialized = System.Text.Json.JsonSerializer.Serialize(new CryptoUtils.PublicKeyHolder(CryptoUtils.UserPublicKey), Modules.DefaultSerializationOptions);

            allModules.Insert(0, new List<string[]>
            {
                new string[]
                {
                    CryptoUtils.FileSignatureGuid,
                    signature,
                    publicKeySerialized
                }
            });

            return System.Text.Json.JsonSerializer.Serialize(allModules, Modules.DefaultSerializationOptions);
        }

        private static System.IO.StreamReader GetConstraintModules(bool rooted)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine();
                sw.WriteLine("Begin TreeViewer;");

                string serializedModules = GetSerializedModules(rooted);

                sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                sw.WriteLine(serializedModules);
                sw.WriteLine("End;");
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);

            return new System.IO.StreamReader(ms);
        }

        private static void SetConstraint(TreeNode tree, TreeNode constraint)
        {
            TreeNode clone = tree.Clone();

            List<string> leafNames = constraint.GetLeafNames();

            List<TreeNode> toBeRemoved = new List<TreeNode>();

            foreach (TreeNode leaf in clone.GetLeaves())
            {
                if (!leafNames.Contains(leaf.Name))
                {
                    toBeRemoved.Add(leaf);
                }
            }

            for (int i = 0; i < toBeRemoved.Count; i++)
            {
                TreeNode parent = toBeRemoved[i];

                while (parent.Children.Count == 0 && parent.Parent != null)
                {
                    clone = clone.Prune(parent, true);
                    parent = parent.Parent;
                }
            }

            HashSet<string> ids = new HashSet<string>(from el in clone.GetChildrenRecursiveLazy() select el.Id);

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (node.Parent == null)
                {
                    if (ids.Contains(node.Id) && constraint.IsRooted())
                    {
                        node.Attributes["ConstraintThickness"] = 3.0;
                    }
                }
                else
                {
                    if (ids.Contains(node.Id))
                    {
                        node.Attributes["ConstraintThickness"] = 3.0;
                    }
                }
            }
        }

        private static Dictionary<string, string> ReadAlignment(string fileName, out bool isProtein)
        {
            bool isFasta = false;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                char firstChar = (char)sr.Read();

                while (char.IsWhiteSpace(firstChar) && !sr.EndOfStream)
                {
                    firstChar = (char)sr.Read();
                }

                if (firstChar == '>')
                {
                    isFasta = true;
                }
                else
                {
                    isFasta = false;
                }
            }

            Dictionary<string, string> alignment;

            if (isFasta)
            {
                alignment = ReadFasta(fileName);
            }
            else
            {
                alignment = ReadPhylip(fileName);
            }

            isProtein = false;

            int length = -1;

            foreach (string seq in alignment.Values)
            {
                if (length < 0)
                {
                    length = seq.Length;
                }
                else
                {
                    if (length != seq.Length)
                    {
                        throw new Exception("The sequences do not all have the same length!");
                    }
                }
            }


            foreach (string seq in alignment.Values)
            {
                foreach (char c in seq)
                {
                    if (!(c < 65 ||
                          c == 'A' ||
                          c == 'a' ||
                          c == 'C' ||
                          c == 'c' ||
                          c == 'G' ||
                          c == 'g' ||
                          c == 'T' ||
                          c == 't' ||
                          c == 'U' ||
                          c == 'u' ||
                          c == 'N' ||
                          c == 'n'))
                    {
                        isProtein = true;
                        break;
                    }
                }

                break;
            }

            return alignment;
        }

        private static Dictionary<string, string> ReadPhylip(string fileName)
        {
            Dictionary<string, string> tbr = new Dictionary<string, string>();

            System.Text.RegularExpressions.Regex whitespace = new System.Text.RegularExpressions.Regex("\\s");

            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                string line = sr.ReadLine();
                line = sr.ReadLine();
                while (!sr.EndOfStream || !string.IsNullOrEmpty(line))
                {
                    int index = whitespace.Match(line).Index;

                    string name = line.Substring(0, index).Trim();
                    string sequence = line.Substring(index).Trim();

                    if (!tbr.TryGetValue(name, out string currSeq))
                    {
                        tbr[name] = sequence;
                    }
                    else
                    {
                        tbr[name] = currSeq + sequence;
                    }

                    line = sr.ReadLine();
                }
            }

            return tbr;
        }

        private static Dictionary<string, string> ReadFasta(string fileName)
        {
            Dictionary<string, string> tbr = new Dictionary<string, string>();

            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                string currSeqName = "";
                string currSeq = "";

                string line = sr.ReadLine();
                while (!sr.EndOfStream || !string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith(">"))
                    {
                        if (!string.IsNullOrEmpty(currSeqName))
                        {
                            tbr.Add(currSeqName, currSeq);
                        }
                        currSeqName = line.Substring(1);
                        currSeq = "";
                    }
                    else
                    {
                        currSeq += line.Trim();
                    }

                    line = sr.ReadLine();
                }
                if (!string.IsNullOrEmpty(currSeqName))
                {
                    tbr.Add(currSeqName, currSeq);
                }
            }

            return tbr;
        }

        float[][] ReadMatrix(string fileName, out string[] names)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                string firstLine = sr.ReadLine();

                int count = int.Parse(firstLine.Trim());

                float[][] tbr = new float[count][];

                for (int j = 0; j < count; j++)
                {
                    tbr[j] = new float[j];
                }

                names = new string[count];


                string line = sr.ReadLine();
                int i = 0;

                System.Text.RegularExpressions.Regex whitespace = new System.Text.RegularExpressions.Regex("\\s+");

                while (!sr.EndOfStream || !string.IsNullOrEmpty(line))
                {
                    System.Text.RegularExpressions.Match nameMatch = whitespace.Match(line);

                    names[i] = line.Substring(0, nameMatch.Index).Trim();

                    int currIndex = nameMatch.Index + nameMatch.Length;

                    for (int j = 0; j < i; j++)
                    {
                        System.Text.RegularExpressions.Match match = whitespace.Match(line, currIndex);
                        
						float value;
						
						if (match.Success)
                        {
                            value = float.Parse(line.Substring(currIndex, match.Index - currIndex));
                        }
                        else
                        {
                            value = float.Parse(line.Substring(currIndex));
                        }
						
                        tbr[i][j] = value;
                        currIndex = match.Index + match.Length;
                    }

                    i++;
                    line = sr.ReadLine();
                }

                return tbr;
            }
        }

    }
}
