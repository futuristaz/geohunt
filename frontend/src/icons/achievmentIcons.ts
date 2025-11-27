import {
  Target,
  Crosshair,
  Focus,
  Trophy,
  Medal,
  HelpCircle,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

export const achievementIcons: Record<string, LucideIcon> = {
  FIRST_GUESS: Target,
  BULLSEYE_100M: Crosshair,
  NEAR_1KM: Focus,
  SCORE_10K: Trophy,
  CLEAN_SWEEP: Medal,
};

// fallback if code is unknown
export const defaultAchievementIcon: LucideIcon = HelpCircle;