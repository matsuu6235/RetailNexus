"use client";

import styles from "./StatusStepBar.module.css";

interface Step {
    status: number;
    label: string;
}

interface StatusStepBarProps {
    steps: Step[];
    currentStatus: number;
    cancelledStatuses?: number[];
}

export default function StatusStepBar({ steps, currentStatus, cancelledStatuses = [91, 92, 93] }: StatusStepBarProps) {
    const isCancelled = cancelledStatuses.includes(currentStatus);
    const currentIndex = steps.findIndex((s) => s.status === currentStatus);

    return (
        <div className={styles.stepBar}>
            {steps.map((step, index) => {
                const isReached = !isCancelled && currentIndex >= index;

                let stepClass = styles.stepPending;
                if (!isCancelled && isReached) {
                    stepClass = styles.stepCompleted;
                }

                return (
                    <div key={step.status} className={`${styles.step} ${stepClass}`}>
                        {index > 0 && (
                            <div className={`${styles.connector} ${isReached ? styles.connectorCompleted : ""}`} />
                        )}
                        <div className={styles.circle}>
                            {isReached ? (
                                <svg className={styles.checkIcon} viewBox="0 0 16 16" fill="none">
                                    <path d="M4 8.5L6.5 11L12 5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                                </svg>
                            ) : (
                                <span className={styles.circleNumber}>{index + 1}</span>
                            )}
                        </div>
                        <span className={styles.label}>{step.label}</span>
                    </div>
                );
            })}

            {isCancelled && (
                <div className={`${styles.step} ${styles.stepCancelled}`}>
                    <div className={styles.connector} />
                    <div className={styles.circle}>
                        <svg className={styles.crossIcon} viewBox="0 0 16 16" fill="none">
                            <path d="M4 4L12 12M12 4L4 12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
                        </svg>
                    </div>
                    <span className={styles.label}>
                        {currentStatus === 91 ? "キャンセル依頼" : currentStatus === 92 ? "キャンセル済" : "キャンセル"}
                    </span>
                </div>
            )}
        </div>
    );
}
