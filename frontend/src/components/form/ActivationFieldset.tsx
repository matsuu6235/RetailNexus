import styles from "@/components/modal/FormModal.module.css";

type ActivationFieldsetProps = {
  currentIsActive: boolean;
  changingActivation: boolean;
  toggle: () => void;
};

export default function ActivationFieldset({ currentIsActive, changingActivation, toggle }: ActivationFieldsetProps) {
  return (
    <fieldset className={styles.field} style={{ border: "1px solid #e2e8f0", borderRadius: "8px", padding: "12px", marginTop: "8px" }}>
      <legend style={{ fontSize: "13px", fontWeight: 600, color: "#0f172a", padding: "0 4px" }}>有効状態の変更</legend>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <span style={{ fontSize: "13px" }}>
          現在の状態: <strong>{currentIsActive ? "有効" : "無効"}</strong>
        </span>
        <button
          type="button"
          onClick={toggle}
          disabled={changingActivation}
          className={styles.submitButton}
          style={currentIsActive ? { backgroundColor: "#dc2626" } : {}}
        >
          {changingActivation ? "変更中..." : currentIsActive ? "無効化する" : "有効化する"}
        </button>
      </div>
    </fieldset>
  );
}
