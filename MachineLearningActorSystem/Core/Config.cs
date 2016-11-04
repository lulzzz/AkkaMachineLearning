using System;
using System.Configuration;
using System.Globalization;

namespace MachineLearningActorSystem.Core
{
    public static class Config
    {
        // Core
        private static int _coreShutdownTimeoutSeconds;

        private static bool? _coreSaveModelBinaries;

        private static bool? _coreEnableExplorerActors;

        private static bool? _coreEnableClassifierActors;

        // Classifiers
        private static int _classifierCrossValidationFolds;

        private static int _classifierBootstrapSubSamples;

        // Explorers
        private static int _explorerMaxIterations;

        private static double _explorerExplorationRate = -1;

        private static double _explorerLearningRate = -1;

        private static double _explorerDiscountRate;

        private static double _explorerMoveReward;

        private static double _explorerWallReward;

        private static double _explorerGoalReward;

        public static int CoreShutdownTimeoutSeconds
        {
            get
            {
                try
                {
                    if (_coreShutdownTimeoutSeconds == 0)
                        _coreShutdownTimeoutSeconds =
                            int.Parse(ConfigurationManager.AppSettings["Core:ShutdownTimeoutSeconds"]);
                }
                catch (Exception)
                {
                    _coreShutdownTimeoutSeconds = 60;
                }

                return _coreShutdownTimeoutSeconds;
            }
        }

        public static bool CoreSaveModelBinaries
        {
            get
            {
                try
                {
                    if (!_coreSaveModelBinaries.HasValue)
                        _coreSaveModelBinaries = ConfigurationManager.AppSettings["Core:SaveModelBinaries"] == "true";
                }
                catch (Exception)
                {
                    _coreSaveModelBinaries = false;
                }

                return _coreSaveModelBinaries.Value;
            }
        }

        public static bool CoreEnableExplorerActors
        {
            get
            {
                try
                {
                    if (!_coreEnableExplorerActors.HasValue)
                        _coreEnableExplorerActors = ConfigurationManager.AppSettings["Core:EnableExplorerActors"] ==
                                                    "true";
                }
                catch (Exception)
                {
                    _coreEnableExplorerActors = false;
                }

                return _coreEnableExplorerActors.Value;
            }
        }

        public static bool CoreEnableClassifierActors
        {
            get
            {
                try
                {
                    if (!_coreEnableClassifierActors.HasValue)
                        _coreEnableClassifierActors = ConfigurationManager.AppSettings["Core:EnableClassifierActors"] ==
                                                      "true";
                }
                catch (Exception)
                {
                    _coreEnableClassifierActors = false;
                }

                return _coreEnableClassifierActors.Value;
            }
        }

        public static int ClassifierCrossValidationFolds
        {
            get
            {
                try
                {
                    if (_classifierCrossValidationFolds == 0)
                        _classifierCrossValidationFolds =
                            int.Parse(ConfigurationManager.AppSettings["Classifier:CrossValidationFolds"]);
                }
                catch (Exception)
                {
                    _classifierCrossValidationFolds = 5;
                }

                return _classifierCrossValidationFolds;
            }
        }

        public static int ClassifierBootstrapSubSamples
        {
            get
            {
                try
                {
                    if (_classifierBootstrapSubSamples == 0)
                        _classifierBootstrapSubSamples =
                            int.Parse(ConfigurationManager.AppSettings["Classifier:BootStrapSubSamples"]);
                }
                catch (Exception)
                {
                    _classifierBootstrapSubSamples = 15;
                }

                return _classifierBootstrapSubSamples;
            }
        }

        public static int ExplorerMaxIterations
        {
            get
            {
                try
                {
                    if (_explorerMaxIterations == 0)
                        _explorerMaxIterations = int.Parse(ConfigurationManager.AppSettings["Explorer:MaxIterations"]);
                }
                catch (Exception)
                {
                    _explorerMaxIterations = 20;
                }

                return _explorerMaxIterations;
            }
        }

        public static double ExplorerExplorationRate
        {
            get
            {
                try
                {
                    if (_explorerExplorationRate == -1)
                        _explorerExplorationRate =
                            double.Parse(ConfigurationManager.AppSettings["Explorer:ExplorationRate"],
                                CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _explorerExplorationRate = 0.5;
                }

                return _explorerExplorationRate;
            }
        }

        public static double ExplorerLearningRate
        {
            get
            {
                try
                {
                    if (_explorerLearningRate == -1)
                        _explorerLearningRate = double.Parse(ConfigurationManager.AppSettings["Explorer:LearningRate"],
                            CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _explorerLearningRate = 0.1;
                }

                return _explorerLearningRate;
            }
        }

        public static double ExplorerDiscountRate
        {
            get
            {
                try
                {
                    if (_explorerDiscountRate == 0)
                        _explorerDiscountRate = double.Parse(ConfigurationManager.AppSettings["Explorer:DiscountRate"],
                            CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _explorerDiscountRate = 1.0;
                }

                return _explorerDiscountRate;
            }
        }

        public static double ExplorerMoveReward
        {
            get
            {
                try
                {
                    if (_explorerMoveReward == 0)
                        _explorerMoveReward = double.Parse(ConfigurationManager.AppSettings["Explorer:MoveReward"],
                            CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _explorerMoveReward = -1.0;
                }

                return _explorerMoveReward;
            }
        }

        public static double ExplorerWallReward
        {
            get
            {
                try
                {
                    if (_explorerWallReward == 0)
                        _explorerWallReward = double.Parse(ConfigurationManager.AppSettings["Explorer:WallReward"],
                            CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _explorerWallReward = -5.0;
                }

                return _explorerWallReward;
            }
        }

        public static double ExplorerGoalReward
        {
            get
            {
                try
                {
                    if (_explorerGoalReward == 0)
                        _explorerGoalReward = double.Parse(ConfigurationManager.AppSettings["Explorer:GoalReward"],
                            CultureInfo.InvariantCulture);
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