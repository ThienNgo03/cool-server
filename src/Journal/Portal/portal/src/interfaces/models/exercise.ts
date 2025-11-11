import type { IBaseModel } from "./base";
import type { IMuscle } from "./muscle";

export interface IExercise extends IBaseModel {
    name: string;
    description: string;
    type: string
    muscles?: IMuscle[];
}
