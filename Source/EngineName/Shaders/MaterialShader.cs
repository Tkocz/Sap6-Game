namespace EngineName.Shaders {

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class MaterialShader {
    internal readonly Effect mEffect;

    private Matrix mModel;
    private EffectParameter mModelParam;

    private Matrix mProj;
    private EffectParameter mProjParam;

    private Matrix mView;
    private EffectParameter mViewParam;

    public Matrix Model {
        get {
            return mModel;
        }

        set {
            mModel = value;
            mModelParam.SetValue(value);
        }
    }

    public Matrix Proj {
        get {
            return mProj;
        }

        set {
            mProj = value;
            mProjParam.SetValue(value);
        }
    }

    public Matrix View {
        get {
            return mView;
        }

        set {
            mView = value;
            mViewParam.SetValue(value);
        }
    }


    public MaterialShader(Effect effect) {
        mEffect = effect;

        mModelParam = effect.Parameters["Model"];
        mProjParam  = effect.Parameters["Proj"];
        mViewParam  = effect.Parameters["View"];
    }
}

}
