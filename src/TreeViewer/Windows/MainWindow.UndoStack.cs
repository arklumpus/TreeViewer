/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2022  Giorgio Bianchini
 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;

namespace TreeViewer
{
    public partial class MainWindow
    {
        public static readonly Avalonia.StyledProperty<bool> CanUndoProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(CanUndo), false);
        public bool CanUndo
        {
            get { return GetValue(CanUndoProperty); }
            set { SetValue(CanUndoProperty, value); }
        }

        public static readonly Avalonia.StyledProperty<bool> CanRedoProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(CanRedo), false);
        public bool CanRedo
        {
            get { return GetValue(CanRedoProperty); }
            set { SetValue(CanRedoProperty, value); }
        }

        private Stack<UndoFrame> UndoStack = new Stack<UndoFrame>();
        private Stack<UndoFrame> RedoStack = new Stack<UndoFrame>();
        private (UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate) CurrentFrameLevel = (UndoFrameLevel.TransformerModule, 0, null);
        private UndoFrame LastComputedUndoFrame = null;

        private bool StopAllUpdates = false;

        public void PushUndoFrame(UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate = null)
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                this.UndoStack.Push(new UndoFrame(this, CurrentFrameLevel.level, CurrentFrameLevel.moduleIndex, CurrentFrameLevel.plotLayersToUpdate));
                this.CurrentFrameLevel = (level, moduleIndex, plotLayersToUpdate);
                this.RedoStack.Clear();
                this.CanUndo = true;
                this.CanRedo = false;
                LastComputedUndoFrame = null;
            }
        }

        public void PrepareUndoFrame(UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate = null)
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                if (LastComputedUndoFrame == null)
                {
                    LastComputedUndoFrame = new UndoFrame(this, CurrentFrameLevel.level, CurrentFrameLevel.moduleIndex, CurrentFrameLevel.plotLayersToUpdate);
                    this.CurrentFrameLevel = (level, moduleIndex, plotLayersToUpdate);
                }
                else
                {
                    UpdateCurrentFrameLevel(level, moduleIndex);
                }

                this.RedoStack.Clear();
                this.CanUndo = true;
                this.CanRedo = false;
            }
        }

        public void CommitUndoFrame()
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                this.UndoStack.Push(LastComputedUndoFrame);
                LastComputedUndoFrame = null;
                this.CanUndo = true;
            }
        }

        public void UpdateCurrentFrameLevel(UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate = null)
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                UndoFrameLevel newLevel = (UndoFrameLevel)Math.Min((int)level, (int)CurrentFrameLevel.level);

                int newModuleIndex;

                if (newLevel == UndoFrameLevel.FurtherTransformationModule || newLevel == UndoFrameLevel.CoordinatesModule)
                {
                    newModuleIndex = 0;
                }
                else if (CurrentFrameLevel.level == level)
                {
                    newModuleIndex = Math.Min(moduleIndex, CurrentFrameLevel.moduleIndex);
                }
                else if (newLevel == CurrentFrameLevel.level)
                {
                    newModuleIndex = CurrentFrameLevel.moduleIndex;
                }
                else
                {
                    newModuleIndex = moduleIndex;
                }

                HashSet<int> newPlotLayersToUpdate = null;

                if (newLevel == UndoFrameLevel.PlotActionModule)
                {
                    newPlotLayersToUpdate = new HashSet<int>();

                    if (CurrentFrameLevel.plotLayersToUpdate != null)
                    {
                        foreach (int i in CurrentFrameLevel.plotLayersToUpdate)
                        {
                            newPlotLayersToUpdate.Add(i);
                        }
                    }

                    if (plotLayersToUpdate != null)
                    {
                        foreach (int i in plotLayersToUpdate)
                        {
                            newPlotLayersToUpdate.Add(i);
                        }
                    }

                    if (newPlotLayersToUpdate.Count == 0)
                    {
                        newPlotLayersToUpdate = null;
                    }
                }

                CurrentFrameLevel = (newLevel, newModuleIndex, newPlotLayersToUpdate);
            }
        }

        public async Task Undo()
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                if (LastComputedUndoFrame != null)
                {
                    CommitUndoFrame();
                }

                if (UndoStack.Count > 0)
                {
                    UndoFrame currentState = new UndoFrame(this, CurrentFrameLevel.level, CurrentFrameLevel.moduleIndex, CurrentFrameLevel.plotLayersToUpdate);

                    UndoFrame newCurrentState = this.UndoStack.Pop();

                    int result = await ApplyUndoFrame(newCurrentState, currentState.Level, currentState.ModuleIndex, currentState.PlotLayersToUpdate);

                    if (result == 0)
                    {
                        this.RedoStack.Push(currentState);
                        CurrentFrameLevel = (newCurrentState.Level, newCurrentState.ModuleIndex, newCurrentState.PlotLayersToUpdate);
                        LastComputedUndoFrame = null;
                    }
                    else if (result == 1)
                    {
                        this.UndoStack.Push(newCurrentState);
                    }
                }

                this.CanUndo = this.UndoStack.Count > 0;
                this.CanRedo = this.RedoStack.Count > 0;
            }
        }

        public async Task Redo()
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                if (RedoStack.Count > 0)
                {
                    UndoFrame currentState = new UndoFrame(this, CurrentFrameLevel.level, CurrentFrameLevel.moduleIndex, CurrentFrameLevel.plotLayersToUpdate);

                    UndoFrame newCurrentState = this.RedoStack.Pop();

                    int result = await ApplyUndoFrame(newCurrentState, newCurrentState.Level, newCurrentState.ModuleIndex, newCurrentState.PlotLayersToUpdate);

                    if (result == 0)
                    {
                        this.UndoStack.Push(currentState);
                        CurrentFrameLevel = (newCurrentState.Level, newCurrentState.ModuleIndex, newCurrentState.PlotLayersToUpdate);
                    }
                    else if (result == 1)
                    {
                        this.RedoStack.Push(newCurrentState);
                    }
                }

                this.CanUndo = this.UndoStack.Count > 0;
                this.CanRedo = this.RedoStack.Count > 0;
            }
        }

        private async Task<int> ApplyUndoFrame(UndoFrame frame, UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate)
        {
            if (GlobalSettings.Settings.EnableUndoStack)
            {
                int levelIndex = (int)level;

                if (levelIndex <= 1)
                {
                    bool missingAttachment = false;

                    for (int i = 0; i < frame.Attachments.Length; i++)
                    {
                        bool found = false;

                        foreach (KeyValuePair<string, Attachment> att in this.StateData.Attachments)
                        {
                            if (att.Value.Id == frame.Attachments[i])
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            missingAttachment = true;
                            break;
                        }
                    }

                    if (missingAttachment)
                    {
                        MessageBox box = new MessageBox("Missing attachment", "An attachment that was removed cannot be restored.\nWould you like to try applying the changes anyways?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog2(this);

                        if (box.Result == MessageBox.Results.No)
                        {
                            return 1;
                        }
                    }
                }

                StopAllUpdates = true;

                if (levelIndex == 0)
                {
                    this.SetTransformerModule(frame.TransformerModule);
                    this.UpdateTransformerParameters(frame.TransformerParameters.DeepClone(false));
                }

                if (levelIndex <= 1)
                {
                    List<string> attachmentsToBeRemoved = new List<string>();
                    foreach (KeyValuePair<string, Attachment> att in this.StateData.Attachments)
                    {
                        if (!frame.Attachments.Contains(att.Value.Id))
                        {
                            attachmentsToBeRemoved.Add(att.Key);
                        }
                    }
                    foreach (string sr in attachmentsToBeRemoved)
                    {
                        this.StateData.Attachments.Remove(sr);
                    }
                }

                if (levelIndex <= 2)
                {
                    int currModuleIndex = level == UndoFrameLevel.FurtherTransformationModule ? moduleIndex : 0;

                    int lastEqual = currModuleIndex - 1;

                    for (int i = currModuleIndex; i < Math.Min(frame.FurtherTransformations.Length, this.FurtherTransformations.Count); i++)
                    {
                        if (frame.FurtherTransformations[i] == this.FurtherTransformations[i])
                        {
                            lastEqual = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    while (this.FurtherTransformations.Count > lastEqual + 1)
                    {
                        this.RemoveFurtherTransformation(this.FurtherTransformations.Count - 1);
                    }

                    for (int i = currModuleIndex; i <= lastEqual; i++)
                    {
                        this.UpdateFurtherTransformationParameters[i](frame.FurtherTransformationParameters[i].DeepClone(false));
                    }

                    StopAllUpdates = false;

                    for (int i = lastEqual + 1; i < frame.FurtherTransformations.Length; i++)
                    {
                        this.AddFurtherTransformation(frame.FurtherTransformations[i]);
                        this.UpdateFurtherTransformationParameters[i](frame.FurtherTransformationParameters[i].DeepClone(false));

                        ProgressWindow window = new ProgressWindow() { ProgressText = "Performing further transformations...", IsIndeterminate = false };
                        window.Steps = 1;
                        _ = window.ShowDialog2(this);

                        await Task.Run(async () =>
                        {
                            await this.UpdateOnlyFurtherTransformations(this.FurtherTransformations.Count - 1, window);
                        });

                        window.Close();
                    }

                    StopAllUpdates = true;
                }

                if (levelIndex <= 3)
                {
                    this.SetCoordinateModule(frame.CoordinatesModule);
                    this.UpdateCoordinatesParameters(frame.CoordinatesParameters.DeepClone(false));
                }

                if (levelIndex <= 4)
                {
                    this.GraphBackground = frame.BackgroundColour;

                    int currModuleIndex = level == UndoFrameLevel.PlotActionModule ? moduleIndex : 0;

                    int lastEqual = currModuleIndex - 1;

                    for (int i = currModuleIndex; i < Math.Min(frame.PlotActionModules.Length, this.PlottingActions.Count); i++)
                    {
                        if (frame.PlotActionModules[i] == this.PlottingActions[i])
                        {
                            lastEqual = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    while (this.PlottingActions.Count > lastEqual + 1)
                    {
                        int index = this.PlottingActions.Count - 1;
                        
                        this.RemovePlottingModule(index);

                        PlotCanvases.RemoveAt(index);
                        LayerTransforms.RemoveAt(index);
                        PlotBounds.RemoveAt(index);

                        SelectionCanvases.RemoveAt(index);

                        FullPlotCanvas.RemoveLayer(index);
                        FullSelectionCanvas.RemoveLayer(index);
                    }

                    for (int i = currModuleIndex; i <= lastEqual; i++)
                    {
                        if (i == currModuleIndex || level != UndoFrameLevel.PlotActionModule || (plotLayersToUpdate != null && plotLayersToUpdate.Contains(i)))
                        {
                            this.UpdatePlottingParameters[i](frame.PlotActionParameters[i].DeepClone(false));
                        }
                    }

                    StopAllUpdates = false;

                    for (int i = lastEqual + 1; i < frame.PlotActionModules.Length; i++)
                    {
                        this.AddPlottingModule(frame.PlotActionModules[i]);
                        this.UpdatePlottingParameters[i](frame.PlotActionParameters[i].DeepClone(false));
                        this.AddPlottingModuleAccessoriesAndUpdate();
                    }

                    StopAllUpdates = true;
                }

                StopAllUpdates = false;

                switch (level)
                {
                    case UndoFrameLevel.TransformerModule:
                        await UpdateTransformedTree();
                        break;
                    case UndoFrameLevel.Attachment:
                        await UpdateFurtherTransformations(0);
                        break;
                    case UndoFrameLevel.FurtherTransformationModule:
                        await UpdateFurtherTransformations(moduleIndex);
                        break;
                    case UndoFrameLevel.CoordinatesModule:
                        await UpdateCoordinates();
                        break;
                    case UndoFrameLevel.PlotActionModule:
                        if (plotLayersToUpdate == null)
                        {
                            if (moduleIndex >= 0 && moduleIndex < PlottingActions.Count)
                            {
                                await UpdatePlotLayer(moduleIndex, true);
                            }
                        }
                        else
                        {
                            await UpdatePlotLayer(moduleIndex, true);
                            foreach (int i in plotLayersToUpdate)
                            {
                                if (i != moduleIndex)
                                {
                                    await UpdatePlotLayer(i, true);
                                }
                            }
                        }
                        break;
                }

                return 0;
            }
            else
            {
                return 0;
            }
        }
    }

    public enum UndoFrameLevel
    {
        TransformerModule = 0,
        Attachment = 1,
        FurtherTransformationModule = 2,
        CoordinatesModule = 3,
        PlotActionModule = 4
    }

    internal class UndoFrame
    {
        public TransformerModule TransformerModule { get; }
        public Dictionary<string, object> TransformerParameters { get; }
        public FurtherTransformationModule[] FurtherTransformations { get; }
        public Dictionary<string, object>[] FurtherTransformationParameters { get; }
        public CoordinateModule CoordinatesModule { get; }
        public Dictionary<string, object> CoordinatesParameters { get; }
        public PlottingModule[] PlotActionModules { get; }
        public Dictionary<string, object>[] PlotActionParameters { get; }
        public string[] Attachments { get; }
        public Colour BackgroundColour { get; }
        public UndoFrameLevel Level { get; }
        public int ModuleIndex { get; }
        public IEnumerable<int> PlotLayersToUpdate { get; }

        public UndoFrame(MainWindow window, UndoFrameLevel level, int moduleIndex, IEnumerable<int> plotLayersToUpdate)
        {
            this.TransformerModule = Modules.TransformerModules[window.TransformerComboBox.SelectedIndex];
            this.TransformerParameters = window.TransformerParameters.DeepClone(true);

            this.Attachments = (from el in window.StateData.Attachments select el.Value.Id).ToArray();

            this.FurtherTransformations = window.FurtherTransformations.ToArray();
            this.FurtherTransformationParameters = (from el in window.FurtherTransformationsParameters select el.DeepClone(true)).ToArray();

            this.CoordinatesModule = Modules.CoordinateModules[window.CoordinatesComboBox.SelectedIndex];
            this.CoordinatesParameters = window.CoordinatesParameters.DeepClone(true);

            this.BackgroundColour = window.GraphBackground;

            this.PlotActionModules = window.PlottingActions.ToArray();
            this.PlotActionParameters = (from el in window.PlottingParameters select el.DeepClone(true)).ToArray();

            this.Level = level;
            this.ModuleIndex = moduleIndex;
            this.PlotLayersToUpdate = plotLayersToUpdate;
        }
    }
}
