using System;
using System.Configuration;

namespace MachineLearningActorSystem.Core
{
    public static class Config
    {
        // Core
        private static int _coreShutdownTimeoutSeconds;

        public static int CoreShutdownTimeoutSeconds
        {
            get
            {
                try
                {
                    if (_coreShutdownTimeoutSeconds == 0)
                    {
                        _coreShutdownTimeoutSeconds = int.Parse(ConfigurationManager.AppSettings["Core:ShutdownTimeoutSeconds"]);
                    }
                }
                catch (Exception)
                {
                    _coreShutdownTimeoutSeconds = 60;
                }

                return _coreShutdownTimeoutSeconds;
            }
        }

        private static bool? _coreSaveModelBinaries;

        public static bool CoreSaveModelBinaries
        {
            get
            {
                try
                {
                    if (!_coreSaveModelBinaries.HasValue)
                    {
                        _coreSaveModelBinaries = ConfigurationManager.AppSettings["Core:SaveModelBinaries"] == "true";
                    }
                }
                catch (Exception)
                {
                    _coreSaveModelBinaries = false;
                }

                return _coreSaveModelBinaries.Value;
            }
        }

        private static bool? _coreEnableExplorerActors;

        public static bool CoreEnableExplorerActors
        {
            get
            {
                try
                {
                    if (!_coreEnableExplorerActors.HasValue)
                    {
                        _coreEnableExplorerActors = ConfigurationManager.AppSettings["Core:EnableExplorerActors"] == "true";
                    }
                }
                catch (Exception)
                {
                    _coreEnableExplorerActors = false;
                }

                return _coreEnableExplorerActors.Value;
            }
        }

        private static bool? _coreEnableClassifierActors;

        public static bool CoreEnableClassifierActors
        {
            get
            {
                try
                {
                    if (!_coreEnableClassifierActors.HasValue)
                    {
                        _coreEnableClassifierActors = ConfigurationManager.AppSettings["Core:EnableClassifierActors"] ==
                                                      "true";
                    }
                }
                catch (Exception)
                {
                    _coreEnableClassifierActors = false;
                }

                return _coreEnableClassifierActors.Value;
            }
        }

        // Classifiers
        private static int _classifierCrossValidationFolds;

        public static int ClassifierCrossValidationFolds
        {
            get
            {
                try
                {
                    if (_classifierCrossValidationFolds == 0)
                    {
                        _classifierCrossValidationFolds =
                            int.Parse(ConfigurationManager.AppSettings["Classifier:CrossValidationFolds"]);
                    }
                }
                catch (Exception)
                {
                    _classifierCrossValidationFolds = 5;
                }

                return _classifierCrossValidationFolds;
            }
        }

        private static int _classifierBootstrapSubSamples;

        public static int ClassifierBootstrapSubSamples
        {
            get
            {
                try
                {
                    if (_classifierBootstrapSubSamples == 0)
                    {
                        _classifierBootstrapSubSamples = int.Parse(ConfigurationManager.AppSettings["Classifier:BootStrapSubSamples"]);
                    }
                }
                catch (Exception)
                {
                    _classifierBootstrapSubSamples = 15;
                }

                return _classifierBootstrapSubSamples;
            }
        }

        // Explorers
        private static int _explorerMaxIterations;

        public static int ExplorerMaxIterations
        {
            get
            {
                try
                {
                    if (_explorerMaxIterations == 0)
                    {
                        _explorerMaxIterations = int.Parse(ConfigurationManager.AppSettings["Explorer:MaxIterations"]);
                    }
                }
                catch (Exception)
                {
                    _explorerMaxIterations = 20;
                }

                return _explorerMaxIterations;
            }
        }

        private static double _explorerExplorationRate;

        public static double ExplorerExplorationRate
        {
            get
            {
                try
                {
                    if (_explorerExplorationRate == 0)
                    {
                        _explorerExplorationRate = double.Parse(ConfigurationManager.AppSettings["Explorer:ExplorationRate"]);
                    }
                }
                catch (Exception)
                {
                    _explorerExplorationRate = 0.5;
                }

                return _explorerExplorationRate;
            }
        }

        private static double _explorerLearningRate = 0.5;

        public static double ExplorerLearningRate
        {
            get
            {
                try
                {
                    if (_explorerLearningRate == 0)
                    {
                        _explorerLearningRate = double.Parse(ConfigurationManager.AppSettings["Explorer:LearningRate"]);
                    }
                }
                catch (Exception)
                {
                    _explorerLearningRate = 0.5;
                }

                return _explorerLearningRate;
            }
        }

        private static double _explorerMoveReward;

        public static double ExplorerMoveReward
        {
            get
            {
                try
                {
                    if (_explorerMoveReward == 0)
                    {
                        _explorerMoveReward = double.Parse(ConfigurationManager.AppSettings["Explorer:MoveReward"]);
                    }
                }
                catch (Exception)
                {
                    _explorerMoveReward = -1.0;
                }

                return _explorerMoveReward;
            }
        }

        private static double _explorerWallReward;

        public static double ExplorerWallReward
        {
            get
            {
                try
                {
                    if (_explorerWallReward == 0)
                    {
                        _explorerWallReward = double.Parse(ConfigurationManager.AppSettings["Explorer:WallReward"]);
                    }
                }
                catch (Exception)
                {
                    _explorerWallReward = -5.0;
                }

                return _explorerWallReward;
            }
        }

        private static double _explorerGoalReward;

        public static double ExplorerGoalReward
        {
            get
            {
                try
                {
                    if (_explorerGoalReward == 0)
                    {
                        _explorerGoalReward = double.Parse(ConfigurationManager.AppSettings["Explorer:GoalReward"]);
                    }
                }
                catch (Exception)
                {
                    _explorerGoalReward = 0.0;
                }

                return _explorerGoalReward;
            }
        }

    }
}
